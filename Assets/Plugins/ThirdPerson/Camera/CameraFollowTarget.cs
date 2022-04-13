using UnityEngine;

namespace ThirdPerson {

/// a follow target that rotates around the player
[ExecuteAlways]
public class CameraFollowTarget: MonoBehaviour {
    // -- lifecycle --
    [Tooltip("the character model position we are trying to follow")]
    [SerializeField] private Transform m_Model;

    [Tooltip("the target we are trying to look at")]
    [SerializeField] private CameraLookAtTarget m_LookAtTarget;

    [Tooltip("the fixed distance from the target")]
    [SerializeField] private float m_Distance;

    [Tooltip("the minimum angle the camera rotates around the character vertically")]
    [SerializeField] private float m_MinAngle;

    [Tooltip("the maximum angle the camera rotates around the character vertically")]
    [SerializeField] private float m_MaxAngle;

    [Tooltip("the output follow target for cinemachine")]
    [SerializeField] private Transform m_Target;

    // -- lifecycle --
    private void Update() {
        var angle = Mathf.LerpAngle(m_MinAngle, m_MaxAngle, m_LookAtTarget.PercentExtended);
        m_Target.position = m_Model.position - Quaternion.AngleAxis(angle, m_Model.right) * m_Model.forward * m_Distance;
    }
}

}