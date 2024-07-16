using UnityEngine;

namespace ThirdPerson {

/// an ik limb for the character model
public sealed class CharacterHead: CharacterBehaviour, CharacterPart {
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
    public override void Init(CharacterContainer c) {
        base.Init(c);

        // set props
        m_HeadBone = c.Rig.Animator.GetBoneTransform(HumanBodyBones.Head);

        // if no head bone, this character has no head, destroy self
        if (!m_HeadBone) {
            Log.Model.W($"destroying head for character: {c.Name}");
            gameObject.SetActive(false);
            return;
        }

        m_CurrRotation = transform.rotation;
    }

    public override void Step_I(float delta) {
        m_IsActive = c.State.Next.IsOnGround;

        // destination rotation follows input
        var destFwd = c.Inputs.Move;
        if (destFwd == Vector3.zero) {
            destFwd = transform.forward;
        }

        m_DestRotation = Quaternion.LookRotation(destFwd, transform.up);

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
    /// .
    public void LookAt(Vector3 target) {
        m_DestRotation = Quaternion.LookRotation(
            Vector3.Normalize(target - m_HeadBone.position),
            m_HeadBone.up
        );
    }

    // -- CharacterPart --
    public void ApplyIk() {
        var anim = c.Rig.Animator;

        anim.SetLookAtWeight(
            m_Weight
        );

        if (m_Weight != 0.0f) {
            anim.SetLookAtPosition(RotToPos(m_CurrRotation));
        }
    }

    public bool MatchesStep(CharacterEvent mask) {
        return false;
    }

    public LimbPlacement Placement {
        get => LimbPlacement.Miss;
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