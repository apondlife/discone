using UnityEngine;

// CharacterState    - the state: pos, speed, heading, etc.
// CharacterMovement - reads input, runs state machine(s), updates state
// CharacterCamera   - it's the camera
// CharacterInput(?) - the input, reads raw input, translates into something

/// the main third person controller
[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [SerializeField] private CharacterController m_CharacterController;



    /// the current phase of the movement system
    /// TODO: is system its own class?
    [SerializeField] private CharacterInput m_Input;
    [SerializeField] private CharacterTunables m_Tunables;
    // serialized so we can see it
    [SerializeField] private CharacterState m_State;

    private Character m_Character;
    private CharacterSystem m_GravitySystem;
    private CharacterSystem m_MovementSystem;
    private CharacterSystem m_JumpSystem;

    private void Awake() {
        m_State = new CharacterState();
        m_Input.Awake();

        // init character
        m_Character = new Character(
            m_Input,
            m_State,
            m_Tunables,
            m_CharacterController
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

        // move the character
        if(m_State.Velocity.magnitude > 0) {
            m_CharacterController.Move(m_State.Velocity * Time.deltaTime);
        }

        m_State.PlanarVelocity = m_CharacterController.velocity;
        m_State.VerticalSpeed = m_CharacterController.velocity.y;
    }
}
