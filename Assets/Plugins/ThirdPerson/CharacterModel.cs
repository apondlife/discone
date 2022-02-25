using UnityEngine;
using Cinemachine;

namespace ThirdPerson {

// needs a reference to ThirdPersonCharacter
public sealed class CharacterModel : MonoBehaviour {
    // -- props --
    /// the character's animator
    private Animator m_Animator;
    /// the character's state
    private CharacterState m_State;

    /// the previous state frame
    /// the character's tunables
    private CharacterTunablesBase m_Tunables;

    // -- lifecycle --
    void Awake() {
        m_Animator = GetComponentInChildren<Animator>();

        var container = GetComponentInParent<ThirdPerson>();
        m_Tunables = container.Tunables;
        m_State = container.State;
    }

    void FixedUpdate() {
        // update animator & model
        SyncAnimator();
        Tilt();
    }

    // -- commands --
    /// sync the animator's params
    void SyncAnimator() {
        if (m_Animator == null) {
            return;
        }

        // set move animation params
        m_Animator.SetFloat(
            "MoveSpeed",
            Mathf.InverseLerp(
                m_Tunables.MinPlanarSpeed,
                m_Tunables.MaxPlanarSpeed,
                m_State.PlanarVelocity.magnitude
            )
        );

        // set jump animation params
        m_Animator.SetBool(
            "JumpSquat",
            m_State.IsInJumpSquat
        );

        m_Animator.SetBool(
            "Airborne",
            !m_State.IsGrounded
        );

        m_Animator.SetFloat(
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