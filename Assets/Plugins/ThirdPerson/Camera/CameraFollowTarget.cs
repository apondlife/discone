using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;

namespace ThirdPerson {

/// a follow target that rotates around the player
public class CameraFollowTarget: MonoBehaviour {
    // -- tunables --
    [Header("tunables")]
    [Tooltip("the fixed distance from the target")]
    [FormerlySerializedAs("m_Distance")]
    [SerializeField] float m_BaseDistance;

    [Tooltip("how much the camera yaws around the character as a fn of angle")]
    [SerializeField] AnimationCurve m_YawCurve;

    [Tooltip("the max speed the camera yaws around the character")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_MaxYawSpeed")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_YawSpeed")]
    [SerializeField] float m_MaxYawSpeed;

    [Tooltip("the acceleration / deceleration of the camera's yaw")]
    [SerializeField] float m_YawAcceleration;

    [Tooltip("the minimum angle the camera rotates around the character vertically")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_MinAngle")]
    [SerializeField] float m_MinPitch;

    [Tooltip("the maximum angle the camera rotates around the character vertically")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_MaxAngle")]
    [SerializeField] float m_MaxPitch;

    [Tooltip("the speed the free look camera yaws")]
    [SerializeField] float m_FreeLook_YawSpeed;

    [Tooltip("the speed the free look camera pitches")]
    [SerializeField] float m_FreeLook_PitchSpeed;

    [Tooltip("the minimum pitch when in free look mode")]
    [SerializeField] float m_FreeLook_MinPitch;

    [Tooltip("the maximum pitch when in free look mode")]
    [SerializeField] float m_FreeLook_MaxPitch;

    [Tooltip("the distance change when undershooting the min pitch angle (gets closer to the character)")]
    [SerializeField] AnimationCurve m_UndershootCurve;

    [Tooltip("the minimum distance from the target, when undershooting")]
    [SerializeField] float m_MinUndershootDistance;

    [Tooltip("the delay in seconds after free look when the camera returns to active mode")]
    [SerializeField] float m_FreeLook_Timeout;

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

    [Tooltip("the output follow target for cinemachine")]
    [SerializeField] Transform m_Target;

    [Tooltip("the free look camera input")]
    [SerializeField] InputActionReference m_FreeLook;

    // -- props --
    /// the current yaw relative to the zero dir
    float m_Yaw = 0.0f;

    /// the current yaw speed
    float m_YawSpeed = 0.0f;

    /// the current pitch
    float m_Pitch = 0.0f;

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

    void Update() {
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
            var pitch = Mathf.LerpAngle(m_Pitch, Mathf.LerpAngle(m_MinPitch, m_MaxPitch, m_LookAtTarget.PercentExtended), 0.5f);

            // update state
            m_Yaw = yaw;
            m_Pitch = pitch;
        }

        // rotate yaw
        var yawRot = Quaternion.AngleAxis(m_Yaw, Vector3.up);

        // rotate pitch on the plane containing the target's position and up
        var pitchAxis = Vector3.Cross(m_Target.forward, Vector3.up).normalized;
        var pitchRot = Quaternion.AngleAxis(m_Pitch, pitchAxis);

        // update the distance if undershooting
        var distance = Mathf.Lerp(m_MinUndershootDistance, m_BaseDistance, m_UndershootCurve.Evaluate(Mathf.InverseLerp(m_FreeLook_MinPitch, m_MinPitch, m_Pitch)));

        // calculate new forward from yaw rotation
        var forward = yawRot * m_ZeroYawDir;
        m_Target.position = m_Model.position + pitchRot * forward * distance;

        // store the forward rotation of the target
        m_Target.forward = forward;
    }

    // -- debug --
    void OnDrawGizmos() {
        var pt = m_Model.position - Vector3.ProjectOnPlane(m_Model.forward, Vector3.up).normalized * m_BaseDistance;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(m_Model.position, 0.1f);
        Gizmos.DrawSphere(pt, 0.1f);
        Gizmos.DrawLine(pt, m_Target.position);
    }
}

}