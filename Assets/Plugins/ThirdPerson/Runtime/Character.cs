using Soil;
using UnityEngine;
using UnityEngine.Serialization;
using System;

namespace ThirdPerson {

/// the main third person controller
public class Character: Character<CharacterInputFrame.Default> {
}

/// the main third person controller
public partial class Character<InputFrame>: MonoBehaviour, CharacterContainer
    where InputFrame: CharacterInputFrame, new() {

    // -- data --
    [Header("data")]
    [Tooltip("the tuning; for tweaking the player's attributes")]
    [SerializeField] CharacterTuning m_Tuning;

    // -- state --
    [Header("state")]
    [Tooltip("if all simulation is paused")]
    [SerializeField] bool m_IsPaused;

    // -- systems --
    [Header("systems")]
    [Tooltip("the idle system")]
    [SerializeField] IdleSystem m_Idle;

    [Tooltip("the surface system")]
    [FormerlySerializedAs("m_Wall")]
    [SerializeField] SurfaceSystem m_Surface;

    [Tooltip("the jump system")]
    [SerializeField] JumpSystem m_Jump;

    [Tooltip("the movement system")]
    [SerializeField] MovementSystem m_Movement;

    [Tooltip("the friction system")]
    [SerializeField] FrictionSystem m_Friction;

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
    CharacterInput<InputFrame> m_Input;

    // TODO: extract the camera out of the character, maybe?
    /// the character's virtual camera
    Camera m_Camera;

    // -- lifecycle --
    protected virtual void Awake() {
        var t = transform;

        // get children
        m_Camera = GetComponentInChildren<Camera>(true);

        // init input (can't access Time from field initializer)
        m_Input = new CharacterInput<InputFrame>();

        // init state
        m_State = new CharacterState(t.position, t.forward, m_Tuning);
        m_Events = new CharacterEvents(m_State);

        // reset rotation; model is the only transform that rotates
        t.rotation = Quaternion.identity;

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
            // TODO: jump actually *does* affect surface, since it cancels inertia; however, running it first would make
            // a frame 1 rejump substantially worse than a frame 2 jump
            m_Surface,
            m_Jump,
            m_Movement,
            // friction system depends on the next frame of forces
            m_Friction,
            // resolves state against the world, runs after all other systems
            m_Collision,
        };

        foreach (var system in m_Systems) {
            system.Init(this);
        }
    }

    protected virtual void Start() {
    }

    protected virtual void Update() {
        #if UNITY_EDITOR
        Debug_Update();
        #endif
    }

    protected virtual void FixedUpdate() {
        var delta = Time.deltaTime;

        // read input
        m_Input.Read(delta);

        // run simulation
        if (!m_IsPaused) {
            // store the previous frame
            m_State.Advance();

            // step systems
            Step();

            // dispatch events
            m_Events.DispatchAll();
        }

        // update external state
        transform.position = m_State.Next.Position;

        // set shader uniforms
        // TODO: re-evaluate this when using it
        var surface = m_State.Next.MainSurface;
        var plane = new Plane(
            surface.IsSome ? surface.Normal : m_State.Next.Up,
            surface.Point
        );

        Shader.SetGlobalVector(
            ShaderProps.CharacterSurfacePlane,
            plane.AsVector4()
        );

        #if UNITY_EDITOR
        Debug_FixedUpdate();
        #endif
    }

    protected virtual void OnDestroy() {
    }

    /// run the character systems
    void Step() {
        var delta = Time.deltaTime;
        foreach (var system in m_Systems) {
            system.Update(delta);
        }
    }

    // -- commands --
    /// drive the character with a new input source
    public void Drive(CharacterInputSource<InputFrame> source) {
        m_Input.Drive(source);
    }

    /// release the current input source, if any
    public void Release() {
        m_Input.Drive(null);
    }

    // TODO: should we fill the full list of frames on connect?
    /// force the current frame's state
    public void ForceState(CharacterState.Frame frame) {
        m_State.Override(frame);
    }

    /// pause the character
    public void Pause() {
        if (!m_IsPaused) {
            OnPause?.Invoke();
        }

        m_IsPaused = true;
    }

    /// unpause the character
    public void Unpause() {
        if (m_IsPaused) {
            OnUnpause?.Invoke();
        }

        m_IsPaused = false;
    }

    // -- queries --
    /// if the character simulation is paused
    public bool IsPaused {
        get => m_IsPaused;
    }

    // -- events --
    /// event for when the character pauses
    public event Action OnPause;

    /// event for when the character unpauses
    public event Action OnUnpause;

    // -- queries --
    /// the character's current state
    public CharacterState.Frame CurrentState {
        get => m_State.Next;
    }

    /// the character's camera
    public Camera Camera {
        get => m_Camera;
    }

    /// .
    public CharacterInput<InputFrame> Input {
        get => m_Input;
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

    // TODO: how should we make state immutable outside the class
    /// .
    public CharacterState State {
        get => m_State;
    }

    /// .
    public CharacterInputQuery Inputs {
        get => m_Input;
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