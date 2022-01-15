using UnityEngine;

// needs a reference to ThirdPersonCharacter
public class CharacterModel: MonoBehaviour {
    // -- props --
    [Tooltip("the character's current state")]
    [SerializeField] private CharacterState m_State;

    [Tooltip("the character's tunables/constants")]
    [SerializeField] private CharacterTunablesBase m_Tunables;

    [Tooltip("the character's animator")]
    [SerializeField] private Animator m_Animator;

    // -- lifecycle --
    void Update() {
        // set move animation params
        m_Animator.SetFloat(
            "MoveSpeed",
            m_State.PlanarSpeed / m_Tunables.MaxPlanarSpeed
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
}
