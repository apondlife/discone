using UnityEngine;

/// the main third person controller
namespace ThirdPerson {

public partial class Character: MonoBehaviour {
    // -- fields --
    [Header("data")]
    [Tooltip("the tunables; for tweaking the player's attributes")]
    [SerializeField] CharacterTunablesBase m_Tunables;

    [Header("children")]
    [Tooltip("the underlying character controller")]
    [SerializeField] CharacterController m_Controller;

    // -- props --
    /// the list of systems acting on this character
    CharacterSystem[] m_Systems;

    /// the character's state
    CharacterState m_State;

    ///the input wrapper
    CharacterInput m_Input = new CharacterInput();

    // -- lifecycle --
    void Awake() {
        // init state
        m_State = new CharacterState(
            transform.position,
            transform.forward
        );

        // init data
        var data = new CharacterData(
            m_Input,
            m_State,
            m_Tunables,
            m_Controller
        );

        // init systems
        m_Systems = new CharacterSystem[] {
            new WallSystem(data),
            new GravitySystem(data),
            new MovementSystem(data),
            new JumpSystem(data),
            new TiltSystem(data),
        };
    }

    void FixedUpdate() {
        // store the previous frame
        m_State.Snapshot();

        // calculate the next one
        var v0 = m_State.Velocity;

        // update the character's systems
        m_Input.Read();
        foreach (var system in m_Systems) {
            system.Update();
        }

        // update controller state from character state
        if (m_State.Velocity.sqrMagnitude > 0) {
            m_Controller.Move(m_State.Position, m_State.Velocity * Time.deltaTime);
        }

        if(m_Controller.Collisions.Count > 0) {
            m_State.Collision = m_Controller.Collisions.Last;
        } else {
            m_State.Collision = null;
        }

        // sync controller state back to character state
        m_State.SetVelocity(m_Controller.Velocity);
        m_State.Position = transform.position;
    }

    // -- commands --
    public void Drive(PlayerInputSource source) {
        m_Input.Drive(source);
    }

    public void ForceState(CharacterState.Frame frame) {
        m_State.SetCurrentFrame(frame);
    }

    // -- queries --
    public CharacterController Controller => m_Controller;

    public CharacterTunablesBase Tunables => m_Tunables;

    // TODO: how should we make state immutable outside the class
    public CharacterState State => m_State;

    // -- events --
    /// when the restart button is pressed, reload the scene
    public void OnRestart() {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            scene.name,
            UnityEngine.SceneManagement.LoadSceneMode.Single
        );
    }
}

}