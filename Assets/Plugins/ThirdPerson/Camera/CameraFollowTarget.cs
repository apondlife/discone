using UnityEngine;

namespace ThirdPerson {
    /// a follow target that rotates around the player
    [ExecuteAlways]
    public class CameraFollowTarget : MonoBehaviour {
        // -- tunables --
        [Header("tunables")]
        [Tooltip("the fixed distance from the target")]
        [SerializeField] private float m_Distance;

        [Tooltip("how much the camera yaws around the character as a fn of angle")]
        [SerializeField] AnimationCurve m_YawCurve;

        [Tooltip("the max speed the camera yaws around the character")]
        [SerializeField] float m_MaxYawSpeed;

        [Tooltip("the minimum angle the camera rotates around the character vertically")]
        [SerializeField] private float m_MinAngle;

        [Tooltip("the maximum angle the camera rotates around the character vertically")]
        [SerializeField] private float m_MaxAngle;

        // -- refs --
        [Header("refs")]
        [Tooltip("the character model position we are trying to follow")]
        [SerializeField] private Transform m_Model;

        [Tooltip("the target we are trying to look at")]
        [SerializeField] private CameraLookAtTarget m_LookAtTarget;

        [Tooltip("the output follow target for cinemachine")]
        [SerializeField] private Transform m_Target;

        // -- props --
        /// the current yaw
        float m_Yaw = 0;

        // -- lifecycle --
        private void Update() {

            // get current & target positions
            var p0 = Vector3.ProjectOnPlane(m_Target.localPosition, m_Model.up);
            var pt = -m_Model.forward * m_Distance;

            // get yaw angle between those two positions
            var yawAngle = Vector3.SignedAngle(p0, pt, transform.up);
            var yawDir = Mathf.Sign(yawAngle);
            var yawMag = Mathf.Abs(yawAngle);

            // we want to rotate around the character's up axis, to maintain the distance
            var yawSpeed = Mathf.Lerp(0, m_MaxYawSpeed, m_YawCurve.Evaluate(yawMag / 180.0f));
            var yawDelta = yawDir * yawSpeed * Time.deltaTime;
            m_Yaw = Mathf.MoveTowardsAngle(m_Yaw, m_Yaw + yawDelta, yawMag);

            var yawRot = Quaternion.AngleAxis(m_Yaw, transform.up);

            // do the pitch rotation later
            var pitch = Mathf.LerpAngle(m_MinAngle, m_MaxAngle, m_LookAtTarget.PercentExtended);
            var pitchAxis = Vector3.Cross(p0, transform.up).normalized;
            var pitchRot = Quaternion.AngleAxis(pitch, pitchAxis);

            m_Target.position = m_Model.position + pitchRot * yawRot * (-Vector3.forward * m_Distance);

        }

        // -- debug --
        private void OnDrawGizmos() {
            var pt = transform.position - m_Model.forward * m_Distance;
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(pt, 0.1f);
            Gizmos.DrawLine(pt, m_Target.position);
        }
    }
}