using UnityEngine;
using UnityEngine.InputSystem;

namespace ThirdPerson {

/// a follow target that rotates around the player
public class CameraFollowTarget: MonoBehaviour {
    // -- tunables --
    [Header("tunables")]
    [Tooltip("the fixed distance from the target")]
    [SerializeField] float m_Distance;

    [Tooltip("how much the camera yaws around the character as a fn of angle")]
    [SerializeField] AnimationCurve m_YawCurve;

    [Tooltip("the max speed the camera yaws around the character")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_MaxYawSpeed")]
    [SerializeField] float m_YawSpeed;

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

    /// the current pitch
    float m_Pitch = 0.0f;

    /// the direction for zero yaw
    Vector3 m_ZeroYawDir;

    /// if the camera is in free look mode
    bool m_FreeLook_Enabled;

    /// the last timestamp when free look was active
    float m_FreeLook_Time;

    // the current character state
    CharacterState m_State;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Yaw = 0;
        m_ZeroYawDir = Vector3.ProjectOnPlane(-m_Model.forward, Vector3.up).normalized;
        m_Pitch = m_MinPitch;
        m_Target.position = transform.position + Quaternion.AngleAxis(m_MinPitch, m_Model.right) * m_ZeroYawDir * m_Distance;
    }

    void Start() {
        // set deps
        var character = GetComponentInParent<Character>();
        m_State = character.State;
    }

    void Update() {
        var time = Time.time;

        // reset the free look time if active
        var freeLook = m_FreeLook.action;
        if (freeLook.IsPressed()) {
            m_FreeLook_Time = time;
        }

        // free look as long as timeout hasn't expired or idle and free looking
        var isIdle = m_State.IdleTime > m_Recenter_IdleTime;
        var isFreeLookExpired = time - m_FreeLook_Time > m_FreeLook_Timeout;
        m_FreeLook_Enabled = m_FreeLook_Enabled && isIdle || !isFreeLookExpired;

        // get current position relative to model
        var p0 = Vector3.ProjectOnPlane(m_Target.position - m_Model.position, Vector3.up);

        // run free look camera if active
        if (m_FreeLook_Enabled) {
            var dir = freeLook.ReadValue<Vector2>();
            m_Yaw += m_FreeLook_YawSpeed * -dir.x * Time.deltaTime;
            m_Pitch += m_FreeLook_PitchSpeed * dir.y * Time.deltaTime;
        }
        // otherwise, run the active/idle camera
        else {
            // get target position relative to model
            var pt = -Vector3.ProjectOnPlane(m_Model.forward, Vector3.up).normalized * m_Distance;

            // get yaw angle between those two positions
            var yawAngle = Vector3.SignedAngle(p0, pt, Vector3.up);
            var yawDir = Mathf.Sign(yawAngle);
            var yawMag = Mathf.Abs(yawAngle);

            // sample yaw speed along recenter / active curve
            var yawSpeed = isIdle
                ? Mathf.Lerp(0, m_Recenter_YawSpeed, m_Recenter_YawCurve.Evaluate(yawMag / 180.0f))
                : Mathf.Lerp(0, m_YawSpeed, m_YawCurve.Evaluate(yawMag / 180.0f));

            // get yaw rotation
            var yawDelta = yawDir * yawSpeed * Time.deltaTime;
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
        var pitchAxis = Vector3.Cross(p0, Vector3.up).normalized;
        var pitchRot = Quaternion.AngleAxis(m_Pitch, pitchAxis);

        // update target position
        m_Target.position = m_Model.position + pitchRot * yawRot * m_ZeroYawDir * m_Distance;
    }

    // -- debug --
    void OnDrawGizmos() {
        var pt = m_Model.position - Vector3.ProjectOnPlane(m_Model.forward, Vector3.up).normalized * m_Distance;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(m_Model.position, 0.1f);
        Gizmos.DrawSphere(pt, 0.1f);
        Gizmos.DrawLine(pt, m_Target.position);
    }
}

}