using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;

namespace ThirdPerson {

/// a follow target that rotates around the player
public class CameraFollowTarget: MonoBehaviour {
    // -- tunables --
    [Header("tunables")]
    [Tooltip("the collision mask for the camera with the world")]
    [SerializeField] private LayerMask m_CollisionMask;

    [Tooltip("the fixed distance from the target")]
    [FormerlySerializedAs("m_Distance")]
    [SerializeField] float m_BaseDistance;

    [Tooltip("how much the camera yaws around the character as a fn of angle")]
    [SerializeField] AnimationCurve m_YawCurve;

    [Tooltip("the max speed the camera yaws around the character")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_MaxYawSpeed")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_YawSpeed")]
    [SerializeField] float m_MaxYawSpeed;

    [Tooltip("the acceleration of the camera yaw")]
    [SerializeField] float m_YawAcceleration;

    [Tooltip("the minimum angle the camera rotates around the character vertically")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_MinAngle")]
    [SerializeField] float m_MinPitch;

    [Tooltip("the maximum angle the camera rotates around the character vertically")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_MaxAngle")]
    [SerializeField] float m_MaxPitch;

    [Tooltip("the maximum pitch speed")]
    [SerializeField] float m_MaxPitchSpeed;

    [Tooltip("the acceleration of the camera pitch")]
    [SerializeField] float m_PitchAcceleration;

    [Tooltip("the rate of change of local distance of the camera to the target")]
    [SerializeField] float m_LocalSpeed;

    [Tooltip("the amount of offset the camera during collision")]
    [SerializeField] float m_ContactOffset;

    // -- target speed --
    [Header("target speed")]
    [Tooltip("the camera distance multiplier as a function of target speed")]
    [FormerlySerializedAs("m_TargetSpeed_DistanceCurve")]
    [SerializeField] AnimationCurve m_Distance_SpeedCurve;

    [Tooltip("the minimum speed for curving camera distance")]
    [FormerlySerializedAs("m_TargetSpeed_MinSpeed")]
    [SerializeField] float m_TargetMinSpeed;

    [Tooltip("the maximum speed for curving camera distance")]
    [FormerlySerializedAs("m_TargetSpeed_MaxSpeed")]
    [SerializeField] float m_TargetMaxSpeed;

    [Tooltip("the maximum speed to camera distance")]
    [SerializeField] float m_TargetSpeed_MaxDistance;

    // -- freelook --
    [Header("freelook")]
    [Tooltip("the speed the free look camera yaws")]
    [SerializeField] float m_FreeLook_YawSpeed;

    [Tooltip("the speed the free look camera pitches")]
    [SerializeField] float m_FreeLook_PitchSpeed;

    [Tooltip("the minimum pitch when in free look mode")]
    [SerializeField] float m_FreeLook_MinPitch;

    [Tooltip("the maximum pitch when in free look mode")]
    [SerializeField] float m_FreeLook_MaxPitch;

    [Tooltip("the distance change when undershooting the min pitch angle (gets closer to the character)")]
    [FormerlySerializedAs("m_UndershootCurve")]
    [SerializeField] AnimationCurve m_Distance_PitchCurve;

    [Tooltip("the minimum distance from the target, when undershooting")]
    [SerializeField] float m_MinUndershootDistance;

    [Tooltip("the delay in seconds after free look when the camera returns to active mode")]
    [SerializeField] float m_FreeLook_Timeout;

    [Tooltip("the delay in seconds after free look when the camera returns to active mode")]
    [SerializeField] float m_FreeLook_OvershootLookUp;

    // -- recenter --
    [Header("recenter")]
    [Tooltip("the time the camera can be idle before recentering")]
    [SerializeField] float m_Recenter_IdleTime;

    [Tooltip("the speed the camera recenters after idle")]
    [SerializeField] float m_Recenter_YawSpeed;

    [Tooltip("the curve the camera recenters looking at the character")]
    [SerializeField] AnimationCurve m_Recenter_YawCurve;

    // -- settings --
    [Header("settings")]
    [Tooltip("if the camera moves around the X axis inverted")]
    [SerializeField] private bool m_InvertX;

    [Tooltip("if the camera moves around the Y axis inverted")]
    [SerializeField] private bool m_InvertY;

    // -- refs --
    [Header("refs")]
    [Tooltip("the character model position we are trying to follow")]
    [SerializeField] Transform m_Model;

    [Tooltip("the target we are trying to look at")]
    [SerializeField] CameraLookAtTarget m_LookAtTarget;

    [Tooltip("the output follow target for cinemachine")]
    [SerializeField] Transform m_Target;

    [Tooltip("the free look camera input")]
    [SerializeField] InputActionReference m_FreeLook;

    // -- props --
    /// the current yaw relative to the zero dir
    [SerializeField] float m_Yaw = 0.0f;

    /// the current yaw speed
    float m_YawSpeed = 0.0f;

    /// the current pitch
    [SerializeField] float m_Pitch = 0.0f;

    /// the current pitch speed
    float m_PitchSpeed = 0.0f;

    /// the direction for zero yaw
    Vector3 m_ZeroYawDir;

    /// if the camera is in free look mode
    bool m_FreeLook_Enabled;

    /// the last timestamp when free look was active
    float m_FreeLook_Time;

    /// if the free look intention is for idle or movement
    bool m_FreeLook_IntendsMovement;

    /// if the player started their intended state
    bool m_FreeLook_StartedIntention;

    /// the current character state
    CharacterState m_State;

    /// the distance the camera is from the target (radius)
    float m_Distance;

    /// storage for raycasts
    RaycastHit m_Hit;

    // -- p/debug
    #if UNITY_EDITOR
    /// the position of the camera pre-collision on the curve
    Vector3 m_CurvePos;

    /// the vision cast hit, if any
    RaycastHit? m_VizHit;

    /// the position of the camera pre-collision on the player's plane
    Vector3 m_PrecastPos;

    /// the project position along the curve
    Vector3 m_ProjPos;

    /// the destination position of the camera post-collision
    Vector3 m_DestPos;

    /// the destination source (proj/viz/player)
    int m_DestSource = 0;
    #endif

    // -- lifecycle --
    void OnValidate() {
        m_MaxPitch = Mathf.Max(m_MinPitch, m_MaxPitch);
        m_FreeLook_MinPitch = Mathf.Min(m_FreeLook_MinPitch, m_MinPitch);
        m_FreeLook_MaxPitch = Mathf.Max(m_FreeLook_MinPitch, m_FreeLook_MaxPitch);
    }

    void Awake() {
        // set props
        m_Yaw = 0;
        m_ZeroYawDir = Vector3.ProjectOnPlane(-m_Model.forward, Vector3.up).normalized;
        m_Pitch = m_MinPitch;
        m_FreeLook_Time = -m_FreeLook_Timeout;

        // set initial position
        m_Target.position = transform.position + Quaternion.AngleAxis(m_MinPitch, m_Model.right) * m_ZeroYawDir * m_BaseDistance;
    }

    void Start() {
        // set deps
        var character = GetComponentInParent<Character>();
        m_State = character.State;
    }

    void FixedUpdate() {
        var time = Time.time;

        // init free look state on first press
        var freeLook = m_FreeLook.action;
        if (freeLook.WasPressedThisFrame()) {
            m_FreeLook_IntendsMovement = false;
            m_FreeLook_StartedIntention = false;
        }

        // reset the free look timeout while input is active
        if (freeLook.IsPressed()) {
            m_FreeLook_Time = time;
        }

        // if player moves at any point during timeout, they set up the camera with
        // the intention of performing a movement
        var isMoving = m_State.IdleTime == 0.0f;
        var isFreeLookExpired = time - m_FreeLook_Time > m_FreeLook_Timeout;
        if (!isFreeLookExpired && isMoving) {
            m_FreeLook_IntendsMovement = true;
        }

        // once the timer expires, the player resolves their intention once they enter,
        // or are already in, their desired state
        if (isFreeLookExpired && !m_FreeLook_StartedIntention) {
            m_FreeLook_StartedIntention = m_FreeLook_IntendsMovement == isMoving;
        }

        // free look as long as the timeout is unexpired, the player hasn't started their
        // intended state, or as long as they're resolving their intended state
        m_FreeLook_Enabled = (
            !isFreeLookExpired ||
            (m_FreeLook_Enabled && !m_FreeLook_StartedIntention) ||
            (m_FreeLook_Enabled && m_FreeLook_IntendsMovement == isMoving)
        );

        // run free look camera if active
        if (m_FreeLook_Enabled) {
            var dir = freeLook.ReadValue<Vector2>();
            dir.x = m_InvertX ? -dir.x : dir.x;
            dir.y = m_InvertY ? -dir.y : dir.y;

            var yaw = m_Yaw;
            yaw += m_FreeLook_YawSpeed * -dir.x * Time.deltaTime;

            var pitch = m_Pitch;
            pitch += m_FreeLook_PitchSpeed * dir.y * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, m_FreeLook_MinPitch, m_FreeLook_MaxPitch);

            m_Yaw = yaw;
            m_Pitch = pitch;
        }
        // otherwise, run the active/idle camera
        else {
            // get target position relative to model
            var pt = -Vector3.ProjectOnPlane(m_Model.forward, Vector3.up).normalized * m_BaseDistance;

            // get yaw angle between those two positions
            var yawAngle = Vector3.SignedAngle(m_Target.forward, pt, Vector3.up);
            var yawDir = Mathf.Sign(yawAngle);
            var yawMag = Mathf.Abs(yawAngle);

            // sample yaw speed along recenter / active curve & accelerate towards it
            var shouldRecenter = m_State.IdleTime > m_Recenter_IdleTime;
            var targetYawSpeed = shouldRecenter
                ? Mathf.Lerp(0, m_Recenter_YawSpeed, m_Recenter_YawCurve.Evaluate(yawMag / 180.0f))
                : Mathf.Lerp(0, m_MaxYawSpeed, m_YawCurve.Evaluate(yawMag / 180.0f));

            m_YawSpeed = Mathf.MoveTowards(m_YawSpeed, targetYawSpeed, m_YawAcceleration * Time.deltaTime);

            // get yaw rotation
            var yawDelta = yawDir * m_YawSpeed * Time.deltaTime;
            var yaw = Mathf.MoveTowardsAngle(m_Yaw, m_Yaw + yawDelta, yawMag);

            // rotate pitch on the plane containing the target's position and up
            var targetPitch = Mathf.LerpAngle(m_MinPitch, m_MaxPitch, m_LookAtTarget.PercentExtended);
            if (m_Pitch == targetPitch) {
                m_PitchSpeed = 0.0f;
            } else  {
                m_PitchSpeed = Mathf.MoveTowards(m_PitchSpeed, m_MaxPitchSpeed, m_PitchAcceleration * Time.deltaTime);
            }

            var pitch = Mathf.MoveTowardsAngle(m_Pitch, targetPitch, m_PitchSpeed * Time.deltaTime);

            // update state
            m_Yaw = yaw;
            m_Pitch = pitch;
        }

        // rotate yaw
        var yawRot = Quaternion.AngleAxis(m_Yaw, Vector3.up);

        // rotate pitch on the plane containing the target's forward and up
        var pitchAxis = Vector3.Cross(m_Target.forward, Vector3.up).normalized;
        var pitchRot = Quaternion.AngleAxis(m_Pitch, pitchAxis);

        // update distance based on target speed
        var multiplier = Mathf.Lerp(
            1.0f,
            m_TargetSpeed_MaxDistance / m_BaseDistance,
            m_Distance_SpeedCurve.Evaluate(Mathf.InverseLerp(
                m_TargetMinSpeed,
                m_TargetMaxSpeed,
                m_State.Curr.PlanarVelocity.magnitude
            ))
        );

        m_Distance = m_BaseDistance * multiplier;

        // calculate new forward from yaw rotation
        var forward = yawRot * m_ZeroYawDir;

        // the camera's ideal location on the pitch curve
        var curvePos = transform.position + pitchRot * forward * m_Distance;

        // find the camera's final pos
        var destPos = FindDestPos(curvePos);

        // store gizmo state
        #if UNITY_EDITOR
        m_DestPos = destPos;
        m_CurvePos = curvePos;
        #endif

        // update target position and forward
        m_Target.position = Vector3.MoveTowards(
            m_Target.position,
            destPos,
            m_LocalSpeed * Time.deltaTime
        );

        m_Target.forward = forward;
    }

    // -- queries --
    /// find the camera's best position given a candidate position
    public Vector3 FindDestPos(Vector3 candidate) {
        // the character's position
        var origin = transform.position;

        // step 1: cast from the character to the ideal position to see if any
        // surface is blocking visibility
        var vizCast = candidate - origin;
        var vizDir = vizCast.normalized;
        var vizLen = vizCast.magnitude;

        var didHit = Physics.Linecast(
            origin,
            candidate,
            out m_Hit,
            m_CollisionMask,
            QueryTriggerInteraction.Ignore
        );

        // store gizmo state
        #if UNITY_EDITOR
        m_VizHit = didHit ? m_Hit : null;
        #endif

        // if the target is visible, we have our desired position
        if (!didHit) {
            return candidate;
        }

        var vizHit = m_Hit;
        var vizPos = vizHit.point;
        var vizNormal = vizHit.normal;

        // aside 1.a: accumulate the normal of all contact surfaces to push the
        // camera away a bit at the end (e.g. skin / contact offset)
        var contactNormal = vizNormal;

        // step 2: if a surface is blocking the camera, cast up and down along
        // the vertical axis containing the ideal position in the direction of
        // the surface.
        var precastDir = Mathf.Sign(vizNormal.y) * Vector3.up;
        var precastLen = 2.0f * m_Distance; // the "diamater" of the curve

        // we're going to use a position on the axis that intersects the surface
        // plane. this has a nice side-effect of allowing the camera to see into
        // enclosed spaces from outside
        var vizPlane = new Plane(vizNormal, vizPos);
        var vizRay = new Ray(m_CurvePos, precastDir);

        // TODO: if we can't intersect, this is a wall. we haven't implemented
        // walls yet
        if (!vizPlane.Raycast(vizRay, out var distance) && distance < precastLen) {
            Debug.LogWarning($"[camera] vision cast into wall @ dist {distance}");
            distance = 0.0f;
        }

        var precastPos = m_CurvePos + precastDir * distance;

        // store gizmo state
        #if UNITY_EDITOR
        m_PrecastPos = precastPos;
        #endif

        // cast up / down along the vertical axis to see if there's a floor or
        // ceiling in our way.
        didHit = Physiics.BounceCast(
            precastPos,
            precastDir,
            out m_Hit,
            precastLen,
            m_CollisionMask,
            QueryTriggerInteraction.Ignore
        );

        // step 3: the projected pos could still be underground or out of view,
        // so we want run a few more casts to the project point find the nearest
        // visible point.
        var projPos = precastPos;
        if (didHit) {
            projPos = m_Hit.point;
        }

        // store gizmo state
        #if UNITY_EDITOR
        m_ProjPos = projPos;
        m_DestSource = 0;
        #endif

        // keep track of the current best dest position and normal, and the
        // distance of the closest option
        var destPos = projPos;
        var destDir = Vector3.zero; // TODO: should m_Hit.normal be used here for contact aggregation?
        var destMinDist = float.MaxValue;

        // do another bounce cast from the position on the player's original
        // visible surface
        didHit = Physiics.RaycastLast(
            vizPos,
            projPos,
            out m_Hit,
            m_CollisionMask,
            QueryTriggerInteraction.Ignore
        );

        // if we hit anything, it's our new best position
        if (didHit) {
            destPos = m_Hit.point;
            destDir = m_Hit.normal;
            destMinDist = Vector3.SqrMagnitude(destPos - projPos);

            #if UNITY_EDITOR
            m_DestSource = 1;
            #endif
        }

        // also do a cast from the player
        didHit = Physics.Linecast(
            origin,
            projPos,
            out m_Hit,
            m_CollisionMask,
            QueryTriggerInteraction.Ignore
        );

        // if the player cast hit
        if (didHit) {
            var nextPos = m_Hit.point;
            var nextDir = m_Hit.normal;
            var nextDist = Vector3.SqrMagnitude(nextPos - projPos);

            // and was closer, it's our new best position
            if (nextDist < destMinDist) {
                destPos = nextPos;
                destDir = nextDir;
                destMinDist = nextDist;

                #if UNITY_EDITOR
                m_DestSource = 2;
                #endif
            }
        }

        // aside 1.b: accumulate this surface normal on the contact normal
        contactNormal = Vector3.Normalize(contactNormal + destDir);

        return destPos + contactNormal * m_ContactOffset;
    }

    public void SetInvertY(bool value) {
        m_InvertY = value;
    }

    public void SetInvertX(bool value) {
        m_InvertX = value;
    }

    /// the target's position
    public Vector3 TargetPosition {
        get => m_Target.position;
    }

    /// the base follow distance
    public float BaseDistance {
        get => m_BaseDistance;
    }

    /// the minimum follow distance
    public float MinDistance {
        get => m_BaseDistance * Mathf.Cos(Mathf.Deg2Rad * m_FreeLook_MinPitch);
    }

    /// if free look is enabled
    public bool IsFreeLookEnabled {
        get => m_FreeLook_Enabled;
    }

    // -- debug --
    #if UNITY_EDITOR
    void OnDrawGizmos() {
        Vector3 r() => Random.insideUnitSphere * 0.05f;

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(m_CurvePos + r(), 0.1f);

        Gizmos.color = Color.Lerp(Color.yellow, Color.red, 0.5f);
        Gizmos.DrawLine(m_CurvePos, transform.position);
        if (m_VizHit == null) {
            return;
        }

        var vizHit = m_VizHit.Value;
        Gizmos.DrawSphere(vizHit.point + r(), 0.1f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(vizHit.point, vizHit.normal);
        Gizmos.DrawSphere(m_PrecastPos + r(), 0.1f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            m_PrecastPos,
            m_PrecastPos + Mathf.Sign(vizHit.normal.y) * 2.0f * m_Distance * Vector3.up
        );

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(m_ProjPos + r(), 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(m_CurvePos, m_DestPos);

        Gizmos.color = (m_DestSource) switch {
            0 => Color.white,
            1 => Color.Lerp(Color.white, Color.magenta, 0.5f),
            _ => Color.Lerp(Color.white, Color.green, 0.5f),
        };

        Gizmos.DrawLine(m_ProjPos, m_DestPos);
        Gizmos.DrawWireSphere(m_DestPos + r(), 0.1f);
    }
    #endif
}

}