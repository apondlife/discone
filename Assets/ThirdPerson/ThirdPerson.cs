using UnityEngine;

/// the main third person controller
[RequireComponent(typeof(CharacterController))]
public partial class ThirdPerson: MonoBehaviour {
    // -- fields --
    [Tooltip("the input wrapper")]
    [SerializeField] private CharacterInput m_Input;

    [Tooltip("the tunables; for tweaking the player's attributes")]
    [SerializeField] private CharacterTunablesBase m_Tunables;

    /// serialized so we can see it
    [SerializeField] private CharacterState m_State;

    [Tooltip("a reference to the underlying character controller")]
    [SerializeField] private CharacterController m_Controller;

    private Character m_Character;
    private CharacterSystem[] m_Systems;

    private void Awake() {
        m_Input.Awake();

        // init character
        m_Character = new Character(
            m_Input,
            m_State,
            m_Tunables,
            m_Controller
        );

        // init systems
        m_Systems = new CharacterSystem[] {
            new GravitySystem(m_Character),
            new MovementSystem(m_Character),
            new JumpSystem(m_Character),
            new TiltSystem(m_Character),
        };
    }

    void FixedUpdate() {
        var v0 = m_State.Velocity;

        // camera to left/forward movement
        m_Input.Update();

        // update the character's systems
        foreach (var system in m_Systems) {
            system.Update();
        }

        // update controller state from character state
        if(m_State.Velocity.magnitude > 0) {
            m_Controller.Move(m_State.Velocity * Time.deltaTime);
        }

        // sync controller state back to character state
        m_State.UpdateVelocity(v0, m_Controller.velocity);
    }
}
