using UnityEngine;

namespace ThirdPerson {

/// the main third person controller
public partial class Character: MonoBehaviour, CharacterContainer {
    // -- data --
    [Header("data")]
    [Tooltip("the tuning; for tweaking the player's attributes")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_Tunables")]
    [SerializeField] CharacterTuning m_Tuning;

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

    [Tooltip("the crouch system")]
    [SerializeField] CrouchSystem m_Crouch;

    [Tooltip("the tilt system")]
    [SerializeField] TiltSystem m_Tilt;

    [Tooltip("the collision system")]
    [SerializeField] CollisionSystem m_Collision;

    // -- children --
    [Header("children")]
    [Tooltip("the character model")]
    [SerializeField] CharacterModel m_Model;

    [Tooltip("the underlying character controller")]
    [SerializeField] CharacterController m_Controller;

    // -- props --
    /// the list of systems acting on this character
    CharacterSystem[] m_Systems;

    /// the character's state
    CharacterState m_State;

    /// the character's state
    CharacterEvents m_Events;

    /// the input wrapper
    CharacterInput m_Input = new();

    // -- lifecycle --
    void Awake() {
        // init state
        m_State = new CharacterState(
            new CharacterState.Frame(
                transform.position,
                transform.forward
            ),
            m_Tuning
        );

        m_Events = new CharacterEvents(m_State);

        // reset rotation; model is the only transform that rotates
        transform.rotation = Quaternion.identity;

        // if editor, stop here
        if (!Application.IsPlaying(gameObject)) {
            return;
        }

        // init controller
        m_Controller.Init();

        // init systems
        m_Systems = new CharacterSystem[] {
            // runs last/first since it depends on real velocity after collision
            m_Idle,
            // these run first, they don't have dependencies
            m_Wall,
            m_Jump,
            m_Crouch,
            // movement system depends on gravity to calculate friciton,
            // so it runs after jump
            m_Movement,
            m_Tilt,
            // resolves state against the world, runs after all other systems
            m_Collision,
        };

        foreach (var system in m_Systems) {
            system.Init(this);
        }
    }

    void FixedUpdate() {
        // run simulation
        if (!m_IsPaused) {
            // store the previous frame
            m_State.Advance();

            // read input
            m_Input.Read();

            // step systems
            Step();
        }

        // dispatch events (even if paused?)
        m_Events.DispatchAll();

        // update external state
        transform.position = m_State.Position;

        // set shader uniforms
        var plane = new Plane(
            m_State.Next.Ground.IsSome ? m_State.Next.Ground.Normal : m_State.Next.Up,
            m_State.Next.Ground.Point
        );

        Shader.SetGlobalVector(
            ShaderProps.CharacterGroundPlane,
            plane.AsVector4()
        );
    }

    /// run the character systems
    void Step() {
        var delta = Time.deltaTime;
        foreach (var system in m_Systems) {
            system.Update(delta);
        }
    }

    // -- props/hot --
    /// if the character simulation is paused
    public bool IsPaused {
        get => m_IsPaused;
    }

    // -- commands --
    /// drive the character with a new input source
    public void Drive(CharacterInputSource source) {
        m_Input.Drive(source);
    }

    /// force the current frame's state
    public void ForceState(CharacterState.Frame frame) {
        // HACK: we should sync the full list of frames on connect
        if (m_State.IsEmpty) {
            m_State.Fill(frame);
        } else {
            m_State.Force(frame);
        }
    }

    /// pause the character
    public void Pause() {
        if (!m_IsPaused) {
            m_Events?.Schedule(CharacterEvent.Paused);
        }

        m_IsPaused = true;
    }

    /// unpause the character
    public void Unpause() {
        if (m_IsPaused) {
            m_Events?.Schedule(CharacterEvent.Unpaused);
        }

        m_IsPaused = false;
    }

    // -- queries --
    /// the character's current state
    public CharacterState.Frame CurrentState {
        get => m_State.Next;
    }

    // -- CharacterContainer --
    /// .
    public string Name {
        get => name;
    }

    /// .
    public CharacterTuning Tuning {
        get => m_Tuning;
    }

    /// .
    public CharacterInput Input {
        get => m_Input;
    }

    // TODO: how should we make state immutable outside the class
    /// .
    public CharacterState State {
        get => m_State;
    }

    /// .
    public CharacterEvents Events {
        get => m_Events;
    }

    /// .
    public CharacterModel Model {
        get => m_Model;
    }

    /// .
    public CharacterController Controller {
        get => m_Controller;
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