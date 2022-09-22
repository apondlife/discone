using UnityEngine;
using System.Linq;

namespace ThirdPerson {

/// the main third person controller
public partial class Character: MonoBehaviour {
    // -- data --
    [Header("data")]
    [Tooltip("the tunables; for tweaking the player's attributes")]
    [SerializeField] CharacterTunablesBase m_Tunables;

    // -- state --
    [Header("state")]
    [Tooltip("if all simulation is paused")]
    [SerializeField] bool m_IsPaused;

    // -- systems --
    [Header("systems")]
    [Tooltip("the idle system")]
    [SerializeField] IdleSystem m_Idle;

    [Tooltip("the wall system")]
    [SerializeField] WallSystem m_Wall;

    [Tooltip("the jump system")]
    [SerializeField] JumpSystem m_Jump;

    [Tooltip("the movement system")]
    [SerializeField] MovementSystem m_Movement;

    [Tooltip("the tilt system")]
    [SerializeField] TiltSystem m_Tilt;

    [Tooltip("the collision system")]
    [SerializeField] CollisionSystem m_Collision;

    // -- children --
    [Header("children")]
    [Tooltip("the underlying character controller")]
    [SerializeField] CharacterController m_Controller;

    // -- props --
    /// the list of systems acting on this character
    CharacterSystem[] m_Systems;

    /// the character's state
    CharacterState m_State;

    /// the character's state
    CharacterEvents m_Events;

    ///the input wrapper
    CharacterInput m_Input = new CharacterInput();

    // -- lifecycle --
    void Awake() {
        // init state
        m_State = new CharacterState(
            transform.position,
            transform.forward
        );

        m_Events = new CharacterEvents(m_State);

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
            m_Controller,
            m_Events
        );

        // init systems
        m_Systems = new CharacterSystem[] {
            // runs last/first since it depends on real velocity after collision
            m_Idle,
            // these run first, they don't have dependencies
            m_Wall,
            m_Jump,
            // movement system depends on gravity to calculate friciton,
            // so it runs nafter jump
            m_Movement,
            m_Tilt,
            // resolves state against the world, runs afte ra
            m_Collision,
        };

        foreach (var system in m_Systems) {
            system.Init(data);
        }
    }

    void FixedUpdate() {
        // run simulation
        if (!m_IsPaused) {
            Simulate();
            m_Events.DispatchAll();
        }

        // update external state
        transform.position = m_State.Position;
    }

    // run character simulation
    void Simulate() {
        // store the previous frame
        m_State.Snapshot();

        // read input
        m_Input.Read();

        // run the character systems
        var delta = Time.deltaTime;
        foreach (var system in m_Systems) {
            system.Update(delta);
        }
    }

    // -- props/hot --
    /// if the character simulation is paused
    public bool IsPaused {
        get => m_IsPaused;
        set => m_IsPaused = value;
    }

    // -- commands --
    /// drive the character with a new input source
    public void Drive(CharacterInputSource source) {
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

    public CharacterEvents Events {
        get => m_Events;
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