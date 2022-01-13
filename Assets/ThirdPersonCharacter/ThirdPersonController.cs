using UnityEngine;

// CharacterState    - the state: pos, speed, heading, etc.
// CharacterMovement - reads input, runs state machine(s), updates state
// CharacterCamera   - it's the camera
// CharacterInput(?) - the input, reads raw input, translates into something

using UnityEngine;

/// the main third person controller
[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [SerializeField] private CharacterController m_CharacterController;



    /// the current phase of the movement system
    /// TODO: is system its own class?
    [SerializeField] private CharacterInput m_Input;
    [SerializeField] private CharacterTunables m_Tunables;

    [SerializeField] private CharacterState m_State =  new CharacterState();

    private MovementSystem m_MovementSystem;
    private Character m_Character;

    private void Awake() {
        // m_Character = new Character();
        m_MovementSystem = new MovementSystem(m_Input, m_State, m_Tunables);
        m_CharacterController = GetComponent<CharacterController>();
    }

    public float PlanarSpeed = 1;
    public float JumpSpeed = 10;

    // JumpSquat => JumpUp => Falling => LandSquat

    void FixedUpdate()
    {
        // camera to left/forward movement
        m_Input.Update();

        m_MovementSystem.Update();

        // this would set the
        m_CharacterController.Move(m_State.Velocity * Time.deltaTime);
    }
}
