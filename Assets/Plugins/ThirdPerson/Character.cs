using UnityEngine;
using System.Linq;

namespace ThirdPerson {

/// the main third person controller
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

        // reset rotation; model is the only transform that rotates
        transform.rotation = Quaternion.identity;

        // if editor, stop here
        if (!Application.IsPlaying(gameObject)) {
            return;
        }

        // init data
        var data = new CharacterData(
            name,
            m_Input,
            m_State,
            m_Tunables,
            m_Controller
        );

        // init systems
        m_Systems = new CharacterSystem[] {
            // has to run first, because it should run after character controller calculations
            new IdleSystem(data),
            new WallSystem(data),
            new GravitySystem(data),
            new MovementSystem(data),
            new JumpSystem(data),
            new TiltSystem(data),
            // the last state, where the controller move is calculated and all move stuff is fixed
            new CollisionSystem(data),
        };
    }

    void FixedUpdate() {
        // run simulation
        if (!IsPaused) {
            Simulate();
        }

        // update external state
        transform.position = m_State.Position;
    }

    // run character simulation
    void Simulate() {
        // store the previous frame
        m_State.Snapshot();

        // update the character's systems
        m_Input.Read();
        foreach (var system in m_Systems) {
            system.Update();
        }
    }

    // -- props/hot --
    /// if the character simulation is paused
    public bool IsPaused { get; set; }

    // -- commands --
    /// drive the character with a new input source
    public void Drive(PlayerInputSource source) {
        m_Input.Drive(source);
    }

    /// force the current frame's state
    public void ForceState(CharacterState.Frame frame) {
        // TODO: hack, we should sync the full list of frames on connect
        if (m_State.IsEmpty) {
            m_State.Fill(frame);
        } else {
            m_State.Force(frame);
        }
    }

    /// pause the character
    public void Pause() {
        IsPaused = true;
    }

    /// unpause the character
    public void Unpause() {
        IsPaused = false;
    }

    // -- queries --
    /// the character's controller
    public CharacterController Controller {
        get => m_Controller;
    }

    /// the character's tunables
    public CharacterTunablesBase Tunables {
        get => m_Tunables;
    }

    /// the character's state
    // TODO: how should we make state immutable outside the class
    public CharacterState State {
        get => m_State;
    }

    /// the character's current state
    public CharacterState.Frame CurrentState {
        get => m_State.Curr;
    }

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