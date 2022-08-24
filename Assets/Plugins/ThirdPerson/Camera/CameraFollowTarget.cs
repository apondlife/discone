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

    [Tooltip("the offset to follow from the character model position")]
    [SerializeField] Vector3 m_Offset;

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

    [Header("Recenter")]
    [Tooltip("the time the camera can be idle before recentering")]
    [SerializeField] float m_Recenter_IdleTime;

    [Tooltip("the speed the camera recenters after idle")]
    [SerializeField] float m_Recenter_YawSpeed;

    [Tooltip("the curve the camera recenters looking at the character")]
    [SerializeField] AnimationCurve m_Recenter_YawCurve;

    [Header("Settings")]
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
    /// the destination position of the camera post-collision
    Vector3 m_DestPos;

    /// the origin of the collision cast
    Vector3 m_CastOrigin;

    /// the position of the camera pre-collision on the curve
    Vector3 m_CurvePos;

    /// the position of the camera pre-collision on the player's plane
    Vector3 m_PrecastPos;

    /// the vision cast hit, if any
    RaycastHit? m_VizHit;

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
            Distance_SpeedMultiplier,
            m_Distance_SpeedCurve.Evaluate(Mathf.InverseLerp(
                m_TargetMinSpeed,
                m_TargetMaxSpeed,
                m_State.Curr.PlanarVelocity.magnitude
            ))
        );

        // update distance based on pitch (for non spherical camera)
        //     multiplier *= Mathf.Lerp(
        //         Distance_PitchMultiplier,
        //         1.0f,
        //         m_Distance_PitchCurve.Evaluate(Mathf.InverseLerp(
        //             -90.0f,
        //             +90.0f,
        //             m_Pitch
        //         ))
        //    );

        m_Distance = m_BaseDistance * multiplier;

        // calculate new forward from yaw rotation
        var forward = yawRot * m_ZeroYawDir;

        // the center of the camera armature
        var curveOrigin = m_Model.position + m_Offset;

        // we did a bunch of math and rotations,
        // and we finally figured out where we want the camera to be on the curve
        // but! there's a world around us, and the curve doesn't acknowledge that!
        m_CurvePos = curveOrigin + pitchRot * forward * m_Distance;

        // before we do anything we want to know if any surface is affecting our visibility
        var vizCast = m_CurvePos - curveOrigin;
        var vizDir = vizCast.normalized;
        var vizLen = vizCast.magnitude;

        var didHit = Physics.Raycast(
            curveOrigin,
            vizDir,
            out m_Hit,
            vizLen,
            m_CollisionMask,
            QueryTriggerInteraction.Ignore
        );

        #if UNITY_EDITOR
        m_VizHit = didHit ? m_Hit : null;
        #endif

        // if the target is visible, we have our desired position
        if (!didHit) {
            m_DestPos = m_CurvePos;
        }
        // otherwise, something is blocking the camera and we need to cast to
        // find a surface for it
        else {
            var vizHit = m_Hit;
            var vizPos = vizHit.point;
            var vizNormal =vizHit.normal;

            // we're going to precast vertically in the direction of normal
            var precastDir = Mathf.Sign(vizNormal.y) * Vector3.up;
            var precastLen = 2.0f * m_Distance; // the "diamater" of the curve

            // precast from a position in the plane of the surface between player and camera
            // along the axis containing the curve pos
            var vizPlane = new Plane(vizNormal, vizPos);
            var vizRay = new Ray(m_CurvePos, precastDir);

            // TODO: if we can't intersect, this is a wall. we haven't implemented walls yet
            if (!vizPlane.Raycast(vizRay, out var distance) && distance < precastLen) {
                Debug.LogWarning($"[camera] vision cast into wall @ dist {distance}");
                distance = 0.0f;
            }

            m_PrecastPos = m_CurvePos + precastDir * distance;

            // now we want to cast from somewhere above ground towards our position in the curve
            // to find if the position in the curve is underground
            // but first, we need to find the point from where to cast
            // so we will cast away from curve position first, to make sure we cast from a safe position
            didHit = Physics.Raycast(
                m_PrecastPos,
                precastDir,
                out m_Hit,
                precastLen,
                m_CollisionMask,
                QueryTriggerInteraction.Ignore
            );

            // now, knowing a safe point to cast from, we do our actual surface finding cast
            // if we hit something, we want to cast back from that point to find the camera's
            // final location. if not, we cast from the farthest point on the precast
            m_CastOrigin = didHit ? m_Hit.point : m_PrecastPos + precastDir * precastLen;

            var castDir = -precastDir;
            var castLen = Vector3.Distance(m_PrecastPos, m_CastOrigin);

            didHit = Physics.Raycast(
                m_CastOrigin,
                castDir,
                out m_Hit,
                castLen,
                m_CollisionMask,
                QueryTriggerInteraction.Ignore
            );

            // if we hit a surface, that's our camera position!
            // otherwise, all this calculation is just thrown away,
            // we are floating in space with no world to collide with
            var pos = didHit ? m_Hit.point : m_PrecastPos;

            // do one final cast from the position on the player's original visible
            // surface to the projected position in the surface plane, making sure it's
            // still visible
            var vizCast2 = pos - vizPos;
            didHit = Physics.Raycast(
                vizPos,
                vizCast2.normalized,
                out m_Hit,
                vizCast2.magnitude,
                m_CollisionMask,
                QueryTriggerInteraction.Ignore
            );

            m_DestPos = didHit ? m_Hit.point : pos;
        }

        // update target position and forward
        m_Target.position = Vector3.MoveTowards(
            m_Target.position,
            m_DestPos,
            m_LocalSpeed * Time.deltaTime
        );

        m_Target.forward = forward;
    }

    // -- queries --
    public void SetInvertY(bool value) {
        m_InvertY = value;
    }

    public void SetInvertX(bool value) {
        m_InvertX = value;
    }

    // -- debug --
    void OnDrawGizmos() {
        var pt = m_Model.position - Vector3.ProjectOnPlane(m_Model.forward, Vector3.up).normalized * m_BaseDistance;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(m_Model.position, 0.1f);
        Gizmos.DrawSphere(pt, 0.1f);
        Gizmos.DrawLine(pt, m_Target.position);

        Vector3 r() => Random.insideUnitSphere * 0.05f;

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(m_CurvePos + r(), 0.1f);

        Gizmos.color = Color.Lerp(Color.yellow, Color.red, 0.5f);
        Gizmos.DrawLine(m_CurvePos, m_Model.position + m_Offset);

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
        Gizmos.DrawSphere(m_CastOrigin + r(), 0.1f);
        Gizmos.DrawLine(m_CastOrigin, m_DestPos);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(m_CurvePos, m_DestPos);
    }

    public float Distance_PitchMultiplier {
        get => m_MinUndershootDistance / m_BaseDistance;
    }

    public float Distance_SpeedMultiplier {
        get => m_TargetSpeed_MaxDistance / m_BaseDistance;
    }
}

}