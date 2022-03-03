using UnityEngine;

namespace ThirdPerson {

/// a container for the character's model and animations
public sealed class CharacterModel: MonoBehaviour {
    // -- fields --
    [Header("references")]
    [Tooltip("The shared third person animator controller")]
    [SerializeField] private RuntimeAnimatorController m_AnimatorController;

    // -- props --
    /// the character's animator
    private Animator m_Animator;

    /// the character's state
    private CharacterState m_State;

    /// the character's tunables
    private CharacterTunablesBase m_Tunables;

    // -- lifecycle --
    void Awake() {
        // get dependencies
        var container = GetComponentInParent<ThirdPerson>();
        m_Tunables = container.Tunables;
        m_State = container.State;

        // configure animator
        m_Animator = GetComponentInChildren<Animator>();
        if (m_Animator != null && m_Animator.runtimeAnimatorController == null) {
            m_Animator.runtimeAnimatorController = m_AnimatorController;
        }
    }

    void FixedUpdate() {
        // update animator & model
        SyncAnimator();
        Tilt();
    }

    // -- commands --
    /// sync the animator's params
    void SyncAnimator() {
        var anim = m_Animator;
        if (anim == null || anim.runtimeAnimatorController == null) {
            return;
        }

        // set move animation params
        anim.SetFloat(
            "MoveSpeed",
            Mathf.InverseLerp(
                m_Tunables.MinPlanarSpeed,
                m_Tunables.MaxPlanarSpeed,
                m_State.PlanarVelocity.magnitude
            )
        );

        // set jump animation params
        anim.SetBool(
            "JumpSquat",
            m_State.IsInJumpSquat
        );

        anim.SetBool(
            "Airborne",
            !m_State.IsGrounded
        );

        anim.SetFloat(
            "VerticalSpeed",
            m_State.VerticalSpeed
        );
    }

    /// tilt the model as a fn of character acceleration
    void Tilt() {
        // is this a fundamental misunderstanding of quaternions?
        transform.rotation = m_State.LookRotation;
    }
}

}