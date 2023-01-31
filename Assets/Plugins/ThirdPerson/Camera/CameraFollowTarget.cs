using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;

namespace ThirdPerson {

/// a follow target that rotates around the player
public class CameraFollowTarget: MonoBehaviour {
    // -- state --
    [Header("spherical coordinates")]
    [Tooltip("the current yaw (rotation around the y-axis); relative to the zero dir")]
    [SerializeField] float m_Yaw = 0.0f;

    [Tooltip("the current pitch (rotation around the x-axis)")]
    [SerializeField] float m_Pitch = 0.0f;

    [Tooltip("the current radius")]
    [SerializeField] float m_Distance = 0.0f;

    // -- tunables --
    [Header("tunables")]
    [Tooltip("the collision mask for the camera with the world")]
    [SerializeField] private LayerMask m_CollisionMask;

    [Tooltip("the fixed distance from the target")]
    [SerializeField] float m_BaseDistance;

    [Tooltip("how much the camera yaws around the character as a fn of angle")]
    [SerializeField] AnimationCurve m_YawCurve;

    [Tooltip("the max speed the camera yaws around the character")]
    [SerializeField] float m_MaxYawSpeed;

    [Tooltip("the acceleration of the camera yaw")]
    [SerializeField] float m_YawAcceleration;

    [Tooltip("the minimum angle the camera rotates around the character vertically")]
    [SerializeField] float m_MinPitch;

    [Tooltip("the maximum angle the camera rotates around the character vertically")]
    [SerializeField] float m_MaxPitch;

    [Tooltip("the maximum pitch speed")]
    [SerializeField] float m_MaxPitchSpeed;

    [Tooltip("the acceleration of the camera pitch")]
    [SerializeField] float m_PitchAcceleration;

    [Tooltip("the rate of change of local distance of the camera to the target, if correcting")]
    [SerializeField] float m_CorrectionSpeed;

    [Tooltip("the smooth time for moving the camera to target, if correcting")]
    [SerializeField] float m_CorrectionSmoothTime = 0.5f;

    // TODO: this is the camera's radius
    // TODO: make all the camera's casts sphere casts
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

    [Tooltip("the acceleration of the camera yaw while in freelook")]
    [SerializeField] float m_FreeLook_YawAcceleration;

    [Tooltip("the speed the free look camera pitches")]
    [SerializeField] float m_FreeLook_PitchSpeed;

    [Tooltip("the acceleration of the camera pitch while in freelook")]
    [SerializeField] float m_FreeLook_PitchAcceleration;

    [Tooltip("the speed the camera distance adjusts in freelook")]
    [SerializeField] float m_FreeLook_DistanceSpeed;

    // TODO: very weird for this to be smaller than min pitch
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
    /// the current yaw speed
    float m_YawSpeed = 0.0f;

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

    /// storage for raycasts
    RaycastHit m_Hit;

    /// target's forward (pre-correction)
    Vector3 m_Forward;

    /// target damping velocity
    Vector3 m_CorrectionVel;

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

        m_Forward = m_Target.forward;
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

            // overwrite current rotation
            var curDir = (m_Target.position - transform.position);
            var curForward = Vector3.ProjectOnPlane(curDir, Vector3.up);

            var yaw = Vector3.SignedAngle(m_ZeroYawDir, curForward, Vector3.up);

            var targetYawSpeed = m_FreeLook_YawSpeed * -dir.x;
            m_YawSpeed = Mathf.MoveTowards(m_YawSpeed, targetYawSpeed, m_FreeLook_YawAcceleration * Time.deltaTime);
            yaw += m_YawSpeed * Time.deltaTime;

            var pitch = Mathf.Rad2Deg * Mathf.Atan2(curDir.y, curForward.magnitude);
            print(pitch);
            m_PitchSpeed = Mathf.MoveTowards(m_PitchSpeed, m_FreeLook_PitchSpeed * dir.y, m_FreeLook_PitchAcceleration * Time.deltaTime);

            pitch = Mathf.MoveTowardsAngle(
                pitch,
                pitch + m_PitchSpeed * Time.deltaTime,
                Mathf.Abs(m_PitchSpeed * Time.deltaTime));
            pitch = Mathf.Clamp(pitch, m_FreeLook_MinPitch, m_FreeLook_MaxPitch);
            print("move " + pitch);

            m_Yaw = yaw;
            m_Pitch = pitch;
            m_Distance = curDir.magnitude;
        }
        // otherwise, run the active/idle/auto camera,
        // this tries to be behind the character
        else {
            // get desired position behind model
            var desiredGroundDir = -Vector3.ProjectOnPlane(m_Model.forward, Vector3.up).normalized;

            var currentGroundDir = Vector3.ProjectOnPlane(m_Forward, Vector3.up).normalized;

            // get yaw angle between current direction and target forward
            var yawAngle = Vector3.SignedAngle(currentGroundDir, desiredGroundDir, Vector3.up);
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
            m_PitchSpeed = Mathf.MoveTowards(m_PitchSpeed, m_MaxPitchSpeed, m_PitchAcceleration * Time.deltaTime);

            var pitch = Mathf.MoveTowardsAngle(m_Pitch, targetPitch, m_PitchSpeed * Time.deltaTime);

            // update state
            m_Yaw = yaw;
            m_Pitch = pitch;
        }

        var multiplier = Mathf.Lerp(
            1.0f,
            m_TargetSpeed_MaxDistance / m_BaseDistance,
            m_Distance_SpeedCurve.Evaluate(Mathf.InverseLerp(
                m_TargetMinSpeed,
                m_TargetMaxSpeed,
                m_State.Next.PlanarVelocity.magnitude
            ))
        );

        var distance = m_BaseDistance * multiplier;
        // because the distance is reset on freelook
        m_Distance = Mathf.MoveTowards(m_Distance, distance, m_FreeLook_DistanceSpeed * Time.deltaTime);

        // find the position in sphere around the model
        var yawRot = Quaternion.AngleAxis(m_Yaw, Vector3.up);
        m_Forward = yawRot * m_ZeroYawDir;

        // rotate pitch on the plane containing the target's forward and up
        var pitchAxis = Vector3.Cross(m_Forward, Vector3.up).normalized;
        var pitchRot = Quaternion.AngleAxis(m_Pitch, pitchAxis);

        // the camera's ideal location on the pitch curve
        var curvePos = transform.position + pitchRot * m_Forward * m_Distance;

        // find the camera's final pos
        var shouldCorrectPosition = !m_FreeLook_Enabled;
        if (shouldCorrectPosition) {
            m_Target.position = Vector3.SmoothDamp(
                m_Target.position,
                GetCorrectedPos(curvePos),
                ref m_CorrectionVel,
                m_CorrectionSmoothTime,
                m_CorrectionSpeed);
        } else {
            m_Target.position = curvePos;
        }
    }

    // -- queries --
    /// correct camera position in attempt to preserve line of sight
    /// see: https://miro.com/app/board/uXjVOWfpI6I=/?moveToWidget=3458764535240497690&cot=14
    private Vector3 GetCorrectedPos(Vector3 candidate) {
        // the final position
        var destPos = candidate;

        // the character's position
        var origin = transform.position;

        // step 1: cast from the character to the ideal position to see if any
        // surface is blocking visibility; use a sphere cast so we don't get
        // closer than the contact offset
        var vizCastStart = candidate - origin;
        var vizLen = vizCastStart.magnitude;
        var vizDir = vizCastStart.normalized;

        var didHit = Physics.SphereCast(
            origin,
            m_ContactOffset,
            vizDir,
            out m_Hit,
            vizLen,
            m_CollisionMask,
            QueryTriggerInteraction.Ignore
        );

        // if the target is visible, we have our desired position
        if (!didHit) {
            return destPos;
        }

        // otherwise, we found the point on this surface (note: offset by c.o.
        // so that step 3b works)
        var vizNormal = m_Hit.normal;
        var vizPos = OffsetHit(m_Hit);

        destPos = vizPos;

        // step 2: project the candidate along the plane of the hit surface
        // using the remaining distance to candidate

        // scale the projection down if the pitch is < 0 so that we can pan
        // into the character
        var projK = 1.0f;
        if (m_Pitch < 0.0f) {
            projK = 1.0f - m_Pitch / m_FreeLook_MinPitch;
        }

        var projLen = Vector3.Distance(candidate, vizPos);
        var projDir = Vector3.Cross(vizNormal, Vector3.Cross(vizDir, vizNormal)).normalized;
        var projPos = vizPos + projK * projLen * projDir;

        destPos = projPos;

        // step 3: we may have projected ourselves into objects or into a place
        // that is occluded (e.g. by a doorframe), so try and escape these
        // problems

        // step 3.a: first, try to exit vertically. if a surface is blocking the
        // camera, cast up and down along the vertical axis containing the ideal
        // position in the direction of the surface.
        var exitVertDir = Mathf.Sign(vizNormal.y) * Vector3.up;
        var distance = Vector3.Distance(candidate, origin);

        var exitVertLen = 2.0f * distance; // the "diamater" of the curve TODO: is this right?

        didHit = Physiics.BounceCast(
            destPos,
            exitVertDir,
            out m_Hit,
            exitVertLen,
            m_CollisionMask,
            QueryTriggerInteraction.Ignore
        );

        if (didHit) {
            destPos = OffsetHit(m_Hit);
        }

        // step 3.b: if we didn't exit vertically, we're still in the viz plane.
        // try to exit along it to deal with occluders like doorways by casting
        // from the viz pos to dest pos.
        if (!didHit) {
            var exitPlaneSrc = vizPos;
            var exitPlaneDst = destPos;

            didHit = Physics.Linecast(
                exitPlaneSrc,
                exitPlaneDst,
                out m_Hit,
                m_CollisionMask,
                QueryTriggerInteraction.Ignore
            );

            if (didHit) {
                destPos = OffsetHit(m_Hit);
            }
        }

        // step 4: do a final vision cast from the player to make sure the destination
        // point is visible.
        var vizCastEndSrc = origin;
        var vizCastEndDst = destPos;

        didHit = Physics.Linecast(
            vizCastEndSrc,
            vizCastEndDst,
            out m_Hit,
            m_CollisionMask,
            QueryTriggerInteraction.Ignore
        );

        if (didHit) {
            destPos = OffsetHit(m_Hit);
        }

        return destPos;
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

    /// the hit point adjusted by the contact offset
    Vector3 OffsetHit(RaycastHit hit) {
        return hit.point + m_ContactOffset * hit.normal;
    }
}

}