using UnityEngine;
using Cinemachine;

namespace ThirdPerson {

// needs a reference to ThirdPersonCharacter
public sealed class CharacterModel : MonoBehaviour {
    // -- fields --
    [Header("references")]
    [Tooltip("the character's animator")]
    [SerializeField] private Animator m_Animator;

    // -- props --
    /// the character's state
    private CharacterState m_State;

    /// the previous state frame
    private CharacterState m_PreviousState;

    /// the character's tunables
    private CharacterTunablesBase m_Tunables;

    // -- lifecycle --
    void Awake() {
        var container = GetComponentInParent<ThirdPerson>();
        m_Tunables = container.Tunables;
        m_State = container.State;
        m_PreviousState = ScriptableObject.Instantiate(m_State);
    }

    void FixedUpdate() {
        // update animator & model
        SyncAnimator();
        Tilt();

        // capture current state
        m_PreviousState = ScriptableObject.Instantiate(m_State);
    }

    // -- commands --
    /// sync the animator's params
    void SyncAnimator() {
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