using UnityEngine;

namespace ThirdPerson {

/// an ik limb for the character model
public sealed class CharacterHead: MonoBehaviour {
    // -- deps --
    /// the containing character
    Character m_Container;

    /// the animator for this limb
    Animator m_Animator;

    // -- tuning --
    [Header("tuning")]
    [Tooltip("the rotation speed of the ik position")]
    [SerializeField] float m_RotationSpeed;

    [Tooltip("the radius of the look sphere")]
    [SerializeField] float m_Radius;

    [Tooltip("the duration of the ik blend when dropping target")]
    [SerializeField] float m_BlendInDuration;

    [Tooltip("the duration of the ik blend when dropping target")]
    [SerializeField] float m_BlendOutDuration;

    // -- props --
    /// if the head is looking towards something
    bool m_IsActive;

    /// the blending weight for this head
    float m_Weight;

    /// the head bone transform; manipulated by ik
    Transform m_HeadBone;

    /// the neck bone transform
    Transform m_NeckBone;

    /// the current ik rotation of the head
    Quaternion m_CurrRotation;

    /// the destination ik rotation of the head
    Quaternion m_DestRotation;

    // -- lifecycle --
    void Awake() {
        // set deps
        m_Container = GetComponentInParent<Character>();
    }

    void Update() {
        var delta = Time.deltaTime;

        m_IsActive = m_Container.State.Next.IsOnGround;

        // destination rotation follows input
        var destFwd = m_Container.Input.Move;
        if (destFwd == Vector3.zero) {
            destFwd = m_NeckBone.forward;
        }

        m_DestRotation = Quaternion.LookRotation(destFwd, m_NeckBone.up);

        // lerp the weight
        var isBlendingIn = m_IsActive;
        m_Weight = Mathf.MoveTowards(
            m_Weight,
            isBlendingIn ? 1.0f : 0.0f,
            delta / (isBlendingIn ? m_BlendInDuration : m_BlendOutDuration)
        );

        // lerp the ik position towards destination
        if (m_IsActive) {
            m_CurrRotation = Quaternion.RotateTowards(
                m_CurrRotation,
                m_DestRotation,
                m_RotationSpeed * Time.deltaTime
            );
        }
    }

    // -- commands --
    /// initialize this limb w/ an animator
    public void Init(Animator animator) {
        // set deps
        m_Animator = animator;

        // set props
        m_HeadBone = m_Animator.GetBoneTransform(HumanBodyBones.Head);
        m_NeckBone = m_HeadBone.parent;
        m_CurrRotation = m_HeadBone.rotation;
    }

    /// .
    public void LookAt(Vector3 target) {
        m_DestRotation = Quaternion.LookRotation(
            Vector3.Normalize(target - m_HeadBone.position),
            m_HeadBone.up
        );
    }

    /// applies the limb ik
    public void ApplyIk() {
        m_Animator.SetLookAtWeight(
            m_Weight
        );

        if (m_Weight != 0.0f) {
            var t = transform;
            m_Animator.SetLookAtPosition(RotToPos(m_CurrRotation));
        }
    }

    // -- queries --
    /// .
    Vector3 RotToPos(Quaternion rot) {
        return m_HeadBone.position + rot * Vector3.forward * m_Radius;
    }

    // -- gizmos --
    void OnDrawGizmos() {
        if (!m_IsActive) {
            return;
        }

        var currPos = RotToPos(m_CurrRotation);
        var destPos = RotToPos(m_DestRotation);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            currPos,
            radius: 0.2f
        );

        Gizmos.DrawLine(
            m_HeadBone.position,
            currPos
        );

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(
            destPos,
            radius: 0.15f
        );

        Gizmos.DrawLine(
            m_HeadBone.position,
            destPos
        );

        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            currPos,
            destPos
        );
    }
}

}