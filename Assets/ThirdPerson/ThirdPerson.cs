using UnityEngine;

/// the main third person controller
[RequireComponent(typeof(CharacterController))]
public partial class ThirdPerson: MonoBehaviour {
    // -- fields --
    [Header("references")]

    [Tooltip("the input wrapper")]
    [SerializeField] CharacterInput m_Input;

    [Tooltip("the tunables; for tweaking the player's attributes")]
    [SerializeField] CharacterTunablesBase m_Tunables;

    [Tooltip("the underlying character controller")]
    [SerializeField] CharacterController m_Controller;

    /// the character's state
    [SerializeField] CharacterState m_State;

    /// the list of systems acting on this character
    private CharacterSystem[] m_Systems;

    /// the last collision between the character and ground
    private ControllerColliderHit m_Hit;

    // -- lifecycle --
    private void Awake() {
        m_Input.Awake();

        // init character
        var character = new Character(
            m_Input,
            m_State,
            m_Tunables,
            m_Controller
        );

        // init systems
        m_Systems = new CharacterSystem[] {
            new GravitySystem(character),
            new MovementSystem(character),
            new JumpSystem(character),
            new TiltSystem(character),
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
            m_Hit = null;
            m_Controller.Move(m_State.Velocity * Time.deltaTime);
        }

        // sync controller state back to character state
        m_State.UpdateVelocity(v0, m_Controller.velocity, m_Hit?.normal);
    }

    // -- events --
    /// when the controller collider contact something
    void OnControllerColliderHit(ControllerColliderHit hit) {
        m_Hit = hit;
    }

    public void OnRestart() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
