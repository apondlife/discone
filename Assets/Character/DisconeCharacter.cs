using Mirror;
using System.Linq;
using ThirdPerson;
using UnityEngine;

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

    // -- state --
    [Header("state")]
    [Tooltip("where the simulation for this character takes place")]
    [SerializeField] Simulation m_Simulation = Simulation.None;

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
    bool m_IsSimulating = false;

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
        // TODO: this if for the camera, it's hacky right now
        m_Simulated = Enumerable.Range(0, transform.childCount)
            .Select((i) => transform.GetChild(i).gameObject)
            .Where((c) => c.activeSelf)
            .ToArray();

        // default to not simulating (note, this relies on the above default values being)
        SetSimulation(Simulation.None);

        // default to not being perceived
        OnIsPerceivedChanged();

        // set initial coordinate, since we are not simulating
        m_Coord.Value = m_Coord.FromPosition(transform.position);

        // debug
        #if UNITY_EDITOR
        Dbg.AddToParent("Characters", this);
        #endif
    }

    void OnDestroy() {
        Debug.Log($"[chrctr] destroying {name}");
        OnSimulationChanged = null;
    }

    void FixedUpdate() {
        // if we simulate this character, send its state to all clients
        if (m_Simulation == Simulation.Local) {
            SendState();
        }
        // otherwise, if the simulation is remote and we're a client, interpolate
        // state to smooth out gaps
        else if (m_InterpolationTime > 0.0f && m_Simulation == Simulation.Remote && isClient) {
            var start = m_Character.State.Curr.Copy();
            var target = m_ReceivedState.Copy();
            var interpolate = target;
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
        // if we don't have authority, do nothing
        if (!netIdentity.IsOwner()) {
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

        if (hasAuthority) {
            Server_SendState(state, m_LastSync);
        }
    }

    // set the character's simulation location
    // TODO: lifecycle events? OnSimulatingLocal/OnSimulatingRemote/OnStopSimulating
    public void SetSimulating(bool isSimulating) {
        m_IsSimulating = isSimulating;
        SyncSimulation();
    }

    // update the character's simulation location
    void SyncSimulation() {
        // simulate locally if owner, remotely otherwise
        var simulation = (m_IsSimulating, netIdentity.IsOwner()) switch {
            (false, _) => Simulation.None,
            (_, true) => Simulation.Local,
            (_, false) => Simulation.Remote,
        };

        // only set if changed
        if (m_Simulation != simulation) {
            SetSimulation(simulation);
        }
    }

    /// force the simulation state; this has a bunch of side-effects, call it cautiously
    void SetSimulation(Simulation simulation) {
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
        // broadcast sync vars
        m_ReceivedState = state;
        m_LastSync = time;

        // place the character in the correct position on the server, since they
        // don't need to interpolate
        m_Character.ForceState(state);
    }

    /// mark this character as unavaialble; only call on the server
    [Server]
    public void Server_AssignClientAuthority(NetworkConnection connection) {
        m_IsAvailable = false;
        netIdentity.AssignClientAuthority(connection);
    }

    /// mark this character as available; only call this on the server
    [Server]
    public void Server_RemoveClientAuthority() {
        m_IsAvailable = true;
        netIdentity.RemoveClientAuthority();
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
    /// when the client receives new state from the server
    [Client]
    void Client_OnStateReceived(CharacterState.Frame prev, CharacterState.Frame next) {
        // if not interpolating, force state
        if (m_InterpolationTime <= 0.0f && m_Simulation == Simulation.Remote) {
            m_Character.ForceState(next);
        }
    }

    // -- e/drive
    /// start driving this character
    public void OnDrive() {
        // don't listen to your own dialogue
        m_Dialogue.StopListening();
    }

    /// release this character
    public void OnRelease() {
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
        get => m_IsSimulating;
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