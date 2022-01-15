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

    [UnityEngine.Serialization.FormerlySerializedAs("m_CharacterController")]
    [Tooltip("a reference to the underlying character controller")]
    [SerializeField] private CharacterController m_Controller;

    private Character m_Character;
    private CharacterSystem m_GravitySystem;
    private CharacterSystem m_MovementSystem;
    private CharacterSystem m_JumpSystem;

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
        m_GravitySystem = new GravitySystem(m_Character);
        m_MovementSystem = new MovementSystem(m_Character);
        m_JumpSystem = new JumpSystem(m_Character);
    }

    void FixedUpdate() {
        // camera to left/forward movement
        m_Input.Update();

        // update the character's systems
        m_GravitySystem.Update();
        m_MovementSystem.Update();
        m_JumpSystem.Update();

        // update controller state from character state
        if(m_State.Velocity.magnitude > 0) {
            m_Controller.Move(m_State.Velocity * Time.deltaTime);
        }

        transform.forward = m_State.FacingDirection;

        // sync controller state back to character state
        m_State.SyncExternalVelocity(m_Controller.velocity);
    }
}
