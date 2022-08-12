using UnityEngine;
using Mirror;
using ThirdPerson;
using System.Linq;

/// an online character
[RequireComponent(typeof(Character))]
[RequireComponent(typeof(CharacterCheckpoint))]
[RequireComponent(typeof(CharacterWrap))]
[RequireComponent(typeof(WorldCoord))]
public sealed class DisconeCharacter: NetworkBehaviour {
    // -- types --
    /// how the character is simulated on the client
    public enum Simulation {
        None, // no simulation
        Remote, // state is received from the server and extrapolated naively
        Local // state is being simulated locally and sent to the server
    }

    // -- fields --
    /// if this character is available
    [Header("fields")]
    [Tooltip("if the character is can be selected initially")]
    [SerializeField] bool m_IsInitial;

    [Tooltip("if the character is currently available")]
    [SyncVar]
    [SerializeField] bool m_IsAvailable = true;

    #if UNITY_EDITOR
    [Tooltip("if this is the initial debug character")]
    [SerializeField] bool m_IsDebug = false;
    #endif

    [Tooltip("the character's most recent state frame")]
    [SyncVar(hook = nameof(Client_OnStateReceived))]
    [SerializeField] CharacterState.Frame m_ReceivedState;

    // -- config --
    [Header("config")]
    [Tooltip("the character's perception")]
    [SerializeField] CharacterPerception m_Perception;

    [Tooltip("how long does the character take to interpolate to the current received state")]
    [SerializeField] float m_InterpolationTime = 0.2f;

    // -- refs --
    [Header("refs")]
    [Tooltip("the character's music")]
    [SerializeField] GameObject m_Music;

    // -- props --
    /// if the character is simulating
    bool m_IsPerceived;

    /// the underlying character
    Character m_Character;

    /// the dialogue
    CharacterDialogue m_Dialogue;

    /// the checkpoint spawner
    CharacterCheckpoint m_Checkpoint;

    /// the world coordinate
    WorldCoord m_Coord;

    /// if the character is currently simulating
    bool m_IsSimulating = true;

    /// where the simulation for this character takes place
    public Simulation m_Simulation = Simulation.Local;

    /// the list of simulated children
    GameObject[] m_Simulated;

    [SyncVar]
    double m_LastSync;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Character = GetComponent<Character>();
        m_Checkpoint = GetComponent<CharacterCheckpoint>();
        m_Dialogue = GetComponentInChildren<CharacterDialogue>();
        m_Coord = GetComponent<WorldCoord>();

        // cache list of simulated children -- anything that's active in the prefab
        // TODO: this if for the camera, it's a bit hacky right now
        m_Simulated = Enumerable.Range(0, transform.childCount)
            .Select((i) => transform.GetChild(i).gameObject)
            .Where((c) => c.activeSelf)
            .ToArray();

        // default to not being perceived
        OnIsPerceivedChanged();

        // debug
        #if UNITY_EDITOR
        Dbg.AddToParent("Characters", this);
        #endif
    }

    void OnDestroy() {
        OnSimulationChanged = null;
    }


    void FixedUpdate() {
        if (m_Simulation == Simulation.Local) {
            SendState();
        } else if (m_Simulation == Simulation.Remote) {
            var interpolate = m_Character.State.Curr.Copy();
            var target = m_ReceivedState.Copy();
            var delta = (float)(NetworkTime.time - m_LastSync);

            // TODO: attempt to also extrapolate...
            // target.Velocity += m_CurrentState.Acceleration * delta;
            // target.Position += target.Velocity * delta;

            var k = Mathf.Clamp01(delta/m_InterpolationTime);
            interpolate.Position = Vector3.Lerp(interpolate.Position, target.Position, k);
            interpolate.Velocity = Vector3.Lerp(interpolate.Velocity, target.Velocity, k);
            interpolate.Acceleration = Vector3.Lerp(interpolate.Acceleration, target.Acceleration, k);

            interpolate.Forward = Vector3.Slerp(interpolate.Forward, target.Forward, k);

            interpolate.Tilt = Quaternion.Slerp(interpolate.Tilt, target.Tilt, k);

            m_Character.ForceState(interpolate);
        }
    }

    // -- l/mirror
    public override void OnStartServer() {
        base.OnStartServer();

        // initially, nobody has authority over any character (except the host client)
        Server_RemoveClientAuthority();
    }

    public override void OnStartAuthority() {
        base.OnStartAuthority();

        // we may move to local simulation if we lose this authority over this character
        SyncSimulation();
    }

    public override void OnStopAuthority() {
        base.OnStopAuthority();

        // we may move to remote simulation if we lose this authority over this character
        SyncSimulation();
    }

    // -- commands --
    /// send state from client -> server, if necessary
    void SendState() {
        var dontSend = !hasAuthority || !isClient;
        // if we don't have authority, do nothing
        if (!hasAuthority || !isClient) {
            return;
        }

        // if the state did not change, do nothing
        var state = m_Character.CurrentState;
        if (m_ReceivedState.Equals(state)) {
            return;
        }

        // sync the current state frame
        m_ReceivedState = state;
        m_LastSync = NetworkTime.time;
        Server_SendState(state, m_LastSync);
    }

    // set the character's simulation location
    // TODO: lifecycle events? OnSimulatingLocal/OnSimulatingRemote/OnStopSimulating
    public void SetSimulating(bool isSimulating) {
        m_IsSimulating = isSimulating;
        SyncSimulation();
    }

    // update the character's simulation location
    void SyncSimulation() {
        var simulation = (m_IsSimulating, hasAuthority) switch {
            (false, _) => Simulation.None,
            (_, true) => Simulation.Local,
            (_, false) => Simulation.Remote,
        };

        // ignore redundant calls
        if (m_Simulation == simulation) {
            return;
        }

        // update state
        m_Simulation = simulation;

        // if the character is simulated at all
        var isSimulated = simulation != Simulation.None;

        // pause when not simulated at all
        m_Character.IsPaused = !isSimulated;
        // TODO: if extrapolating might not need to simulate locally at all
        // m_Character.IsPaused = simulation != Simulation.Local;

        // toggle activity on all the children to turn off rendering, effects, &c
        foreach (var c in m_Simulated) {
            c.SetActive(isSimulated);
        }

        OnSimulationChanged?.Invoke(m_Simulation);
    }

    // -- c/server
    /// sync this character's current state from the client
    [Command]
    void Server_SendState(CharacterState.Frame state, double time) {
        m_ReceivedState = state;
        m_LastSync = time;
    }

    /// mark this character as unavaialble; only call on the server
    [Server]
    public void Server_AssignClientAuthority(NetworkConnection connection) {
        m_IsAvailable = false;
        netIdentity.RemoveClientAuthority();
        netIdentity.AssignClientAuthority(connection);
    }

    /// mark this character as available; only call this on the server
    [Server]
    public void Server_RemoveClientAuthority() {
        m_IsAvailable = true;
        netIdentity.RemoveClientAuthority();
        netIdentity.AssignClientAuthority(NetworkServer.localConnection);
    }

    // -- props/hot --
    /// if the character is perceived
    public bool IsPerceived {
        get => m_IsPerceived;
        set {
            if (m_IsPerceived != value) {
                m_IsPerceived = value;
                OnIsPerceivedChanged();
            }
        }
    }

    // -- events --
    /// when the perceived state changes
    public delegate void SimulationChangedEvent(Simulation sim);

    public SimulationChangedEvent OnSimulationChanged;
    void OnIsPerceivedChanged() {
        // TODO: run this through the EntityCollisions
        m_Music.SetActive(m_IsPerceived);
    }

    // -- e/client
    void Client_OnStateReceived(CharacterState.Frame src, CharacterState.Frame dst) {
        // ignore state if we have authority
        if (hasAuthority) {
            return;
        }

        // update character's current state frame
        // m_Character.ForceState(dst);
    }

    // -- e/drive
    /// start driving this character
    public void OnDrive() {
        // don't listen to your own dialogue
        m_Dialogue.StopListening();
    }

    /// release this character
    public void OnRelease() {
        // start listening again
        m_Dialogue.StartListening();
    }

    // -- queries --
    /// if this character is available
    public bool IsAvailable {
        get => m_IsAvailable;
    }

    /// if the character is selected initially
    public bool IsInitial {
        get => m_IsInitial;
    }

    /// if the character is simulating
    public bool IsSimulating {
        get => m_Simulation != Simulation.None;
    }

    /// the third person character
    public ThirdPerson.Character Character {
        get => m_Character;
    }

    /// the character checkpoint
    public CharacterCheckpoint Checkpoint {
        get => m_Checkpoint;
    }

    /// the character's perception
    public CharacterPerception Perception {
        get => m_Perception;
    }

    /// the world coord
    public WorldCoord Coord {
        get => m_Coord;
    }

    // -- q/debug
    #if UNITY_EDITOR
    /// if this is the debug character
    public bool IsDebug {
        get => m_IsDebug;
    }
    #endif
}