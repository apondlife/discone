using UnityEngine;

namespace ThirdPerson {

/// an ik limb for the character model
public sealed class CharacterLimb: MonoBehaviour {
    // -- deps --
    /// the animator for this limb
    Animator m_Animator;

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the type of goal of this limb")]
    [SerializeField] AvatarIKGoal m_Goal;

    // -- tuning --
    [Header("tuning")]
    [Tooltip("the move speed of the ik position")]
    [SerializeField] float m_MoveSpeed;

    [Tooltip("the turn speed of the ik rotation")]
    [SerializeField] float m_TurnSpeed;

    [Tooltip("the duration of the ik blend when dropping target")]
    [SerializeField] float m_BlendDuration;

    [UnityEngine.Serialization.FormerlySerializedAs("m_MaxDistance")]
    [Tooltip("the max distance before searching for a new dest")]
    [SerializeField] float m_StrideLength;

    // -- props --
    /// if the limb is moving towards something
    bool m_HasTarget;

    /// the blending weight for this limb
    float m_Weight;

    /// the current ik position of the limb
    Vector3 m_CurrPosition;

    /// the current ik rotation of the limb
    Quaternion m_CurrRotation;

    /// the destination ik position of the limb
    Vector3 m_DestPosition;

    /// the destination ik rotation of the limb
    Quaternion m_DestRotation;

    /// the square stride length
    float m_SqrStrideLength;

    // -- lifecycle --
    void Start() {
        // cache stride length
        m_SqrStrideLength = m_StrideLength * m_StrideLength;
    }

    void Update() {
        var delta = Time.deltaTime;

        // lerp the ik position towards destination
        m_CurrPosition = Vector3.MoveTowards(
            m_CurrPosition,
            transform.InverseTransformPoint(m_DestPosition),
            m_MoveSpeed * Time.deltaTime
        );

        // lerp the weight down when there's no target
        m_Weight = Mathf.MoveTowards(
            m_Weight,
            m_HasTarget ? 1.0f : 0.0f,
            delta / m_BlendDuration
        );
    }

    // -- commands --
    /// initialize this limb w/ an animator
    public void Init(Animator animator) {
        m_Animator = animator;
    }

    /// applies the limb ik
    public void ApplyIk(bool isIkActive) {
        if (!isIkActive || !IsActive) {
            m_Animator.SetIKPositionWeight(m_Goal, 0f);
            m_Animator.SetIKRotationWeight(m_Goal, 0f);
            return;
        }

        m_Animator.SetIKPosition(
            m_Goal,
            transform.TransformPoint(m_CurrPosition)
        );

        m_Animator.SetIKPositionWeight(
            m_Goal,
            m_Weight
        );

        m_Animator.SetIKRotation(
            m_Goal,
            m_CurrRotation
        );

        m_Animator.SetIKRotationWeight(
            m_Goal,
            m_Weight
        );
    }

    // -- queries --
    /// if this is a foot
    public bool IsFoot {
        get => m_Goal switch {
            AvatarIKGoal.LeftFoot => true,
            AvatarIKGoal.RightFoot => true,
            _ => false
        };
    }

    /// if the ik is currently active for this limb
    bool IsActive {
        get => gameObject.activeSelf && m_Weight > 0.0f;
    }

    /// the bone for this ik goal
    HumanBodyBones GoalBone {
        get => m_Goal switch {
            AvatarIKGoal.RightHand => HumanBodyBones.RightHand,
            AvatarIKGoal.LeftHand => HumanBodyBones.LeftHand,
            AvatarIKGoal.RightFoot => HumanBodyBones.RightFoot,
            _ /*AvatarIKGoal.LeftFoot*/ => HumanBodyBones.LeftFoot,
        };
    }

    /// if we've completed a stride
    bool HasCompletedStride(Vector3 pos) {
        return Vector3.SqrMagnitude(pos - m_DestPosition) > m_SqrStrideLength;
    }

    // -- events --
    void OnTriggerEnter(Collider other) {
        var pos = other.ClosestPoint(transform.position);
        if (!IsActive || HasCompletedStride(pos)) {
            // set current position from the bone's current position in our local space
            m_CurrPosition = transform.InverseTransformPoint(
                m_Animator.GetBoneTransform(GoalBone).position
            );

            // move towards the closest point on sruface
            m_DestPosition = pos;
        }

        // activate ik
        m_HasTarget = true;
    }

    void OnTriggerStay(Collider other) {
        m_HasTarget = true;

        var pos = other.ClosestPoint(transform.position);
        if (HasCompletedStride(pos)) {
            m_DestPosition = pos;
        }
    }

    void OnTriggerExit(Collider other) {
        m_HasTarget = false;
    }

    // -- gizmos --
    void OnDrawGizmos() {
        if (!m_HasTarget) {
            return;
        }

        var currPos = transform.TransformPoint(m_CurrPosition);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(
            currPos,
            radius: 0.05f
        );

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(
            m_DestPosition,
            radius: 0.05f
        );

        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            currPos,
            m_DestPosition
        );
    }
}

}