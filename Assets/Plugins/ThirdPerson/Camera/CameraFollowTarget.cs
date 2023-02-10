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
    [Tooltip("the tuning parameters for the camera")]
    [SerializeField] private CameraTuning m_Tuning;

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

    // -- refs --
    [Header("refs")]
    [Tooltip("the character model position we are trying to follow")]
    [SerializeField] Transform m_Model;

    [Tooltip("the target we are trying to look at")]
    [SerializeField] CameraLookAtTarget m_LookAtTarget;

    [Tooltip("the output follow destination for cinemachine")]
    [SerializeField] Transform m_Destination;

    [Tooltip("the free look camera input")]
    [FormerlySerializedAs("m_FreeLook")]
    [SerializeField] InputActionReference m_Input;

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


    /// target damping velocity
    Vector3 m_CorrectionVel;

    /// the camera following sytem (state machine)
    CameraFollowSystem m_FollowSystem;

    // -- lifecycle --

    void Awake() {
        // var zeroYawDir = Vector3.ProjectOnPlane(-m_Model.forward, Vector3.up).normalized;


        // set props
        m_Yaw = 0;
        m_ZeroYawDir = Vector3.ProjectOnPlane(-m_Model.forward, Vector3.up).normalized;
        m_Pitch = m_MinPitch;
        m_FreeLook_Time = -m_FreeLook_Timeout;

        // set initial position
        m_Destination.position = transform.position + Quaternion.AngleAxis(m_MinPitch, m_Model.right) * m_ZeroYawDir * m_BaseDistance;

    }

    void Start() {
        // set deps
        var character = GetComponentInParent<Character>();
        m_State = character.State;

        m_FollowSystem = new CameraFollowSystem(
            m_Input.action,
            m_Tuning,
            m_State,
            transform.localPosition
        );

        m_FollowSystem.Init();
    }

    void FixedUpdate() {
        var delta = Time.deltaTime;
        m_FollowSystem.m_CurrPosition = m_Destination.position;
        m_FollowSystem.Update(delta);
        m_Destination.position = m_FollowSystem.DestPosition();

        // // find the camera's final pos
        // var shouldCorrectPosition = !m_FreeLook_Enabled;
        // if (shouldCorrectPosition) {
        //     m_Destination.position = Vector3.SmoothDamp(
        //         m_Destination.position,
        //         GetCorrectedPos(curvePos),
        //         ref m_CorrectionVel,
        //         m_CorrectionSmoothTime,
        //         m_CorrectionSpeed);
        // } else {
        //     m_Destination.position = curvePos;
        // }
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

    public void SetInvertX(bool value) {
        m_Tuning.IsInvertedX = value;
    }

    public void SetInvertY(bool value) {
        m_Tuning.IsInvertedY = value;
    }

    /// the target's position
    public Vector3 TargetPosition {
        get => m_Destination.position;
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