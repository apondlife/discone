using UnityEngine;

namespace ThirdPerson {

/// a follow target that rotates around the player
public class CameraFollowTarget : MonoBehaviour {
    // -- tunables --
    [Header("tunables")]
    [Tooltip("the fixed distance from the target")]
    [SerializeField] private float m_Distance;

    [Tooltip("how much the camera yaws around the character as a fn of angle")]
    [SerializeField] AnimationCurve m_YawCurve;

    [Tooltip("the max speed the camera yaws around the character")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_MaxYawSpeed")]
    [SerializeField] float m_YawSpeed;

    [Tooltip("the minimum angle the camera rotates around the character vertically")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_MinAngle")]
    [SerializeField] private float m_MinPitch;

    [Tooltip("the maximum angle the camera rotates around the character vertically")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_MaxAngle")]
    [SerializeField] private float m_MaxPitch;

    [Tooltip("the time the camera can be idle before recentering")]
    [SerializeField] private float m_Recenter_IdleTime;

    [Tooltip("the speed the camera recenters after idle")]
    [SerializeField] private float m_Recenter_YawSpeed;

    [Tooltip("the curve the camera recenters looking at the character")]
    [SerializeField] AnimationCurve m_Recenter_YawCurve;

    // -- refs --
    [Header("refs")]
    [Tooltip("the character model position we are trying to follow")]
    [SerializeField] private Transform m_Model;

    [Tooltip("the target we are trying to look at")]
    [SerializeField] private CameraLookAtTarget m_LookAtTarget;

    [Tooltip("the output follow target for cinemachine")]
    [SerializeField] private Transform m_Target;

    // -- props --
    /// the current yaw relative to the zero dir
    float m_Yaw = 0.0f;

    /// the current pitch
    float m_Pitch = 0.0f;

    /// the direction for zero yaw
    Vector3 m_ZeroYawDir;

    // the current character state
    CharacterState m_State;

    // -- lifecycle --
    private void Start() {
        m_Yaw = 0;
        m_ZeroYawDir = -Vector3.ProjectOnPlane(m_Model.forward, Vector3.up).normalized;
        m_Pitch = m_MinPitch;
        m_Target.position = transform.position + Quaternion.AngleAxis(m_MinPitch, m_Model.right) * m_ZeroYawDir * m_Distance;

        var character = GetComponentInParent<Character>();
        m_State = character.State;
    }

    private void Update() {

        // get current & target positions
        var p0 = Vector3.ProjectOnPlane(m_Target.position - m_Model.position, Vector3.up);
        var pt = Vector3.ProjectOnPlane(-m_Model.forward, Vector3.up).normalized * m_Distance;

        // get yaw angle between those two positions
        var yawAngle = Vector3.SignedAngle(p0, pt, transform.up);
        var yawDir = Mathf.Sign(yawAngle);
        var yawMag = Mathf.Abs(yawAngle);

        // check if the character has been idle for long enough
        var yawSpeed = m_State.IdleTime > m_Recenter_IdleTime
            ? Mathf.Lerp(0, m_Recenter_YawSpeed, m_Recenter_YawCurve.Evaluate(yawMag / 180.0f))
            : Mathf.Lerp(0, m_YawSpeed, m_YawCurve.Evaluate(yawMag / 180.0f));

        var yawDelta = yawDir * yawSpeed * Time.deltaTime;
        var yaw = Mathf.MoveTowardsAngle(m_Yaw, m_Yaw + yawDelta, yawMag);
        var yawRot = Quaternion.AngleAxis(yaw, Vector3.up);

        // rotate pitch on the plane containing the target's position and up
        var pitch = Mathf.LerpAngle(m_Pitch, Mathf.LerpAngle(m_MinPitch, m_MaxPitch, m_LookAtTarget.PercentExtended), 0.5f);
        var pitchAxis = Vector3.Cross(p0, Vector3.up).normalized;
        var pitchRot = Quaternion.AngleAxis(pitch, pitchAxis);

        // update state
        m_Yaw = yaw;
        m_Pitch = pitch;
        m_Target.position = m_Model.position + pitchRot * yawRot * m_ZeroYawDir * m_Distance;
    }

    // -- debug --
    private void OnDrawGizmos() {
        var pt = m_Model.position - Vector3.ProjectOnPlane(m_Model.forward, Vector3.up).normalized * m_Distance;
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(pt, 0.1f);
        Gizmos.DrawLine(pt, m_Target.position);
    }
}

}