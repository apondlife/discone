using Mirror;
using System.Linq;
using ThirdPerson;
using UnityAtoms;
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

    // -- id --
    [Header("id")]
    [Tooltip("the character's key")]
    [SerializeField] CharacterKey m_Key;

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
    [UnityEngine.Serialization.FormerlySerializedAs("m_ReceivedState")]
    [SerializeField] CharacterState.Frame m_RemoteState;

    // -- cfg --
    [Header("cfg")]
    [Tooltip("how long does the character take to interpolate to the current received state")]
    [SerializeField] float m_InterpolationTime = 0.2f;


    [Header("published")]
    [Tooltip("the character spawning event")]
    [SerializeField] DisconeCharacterEvent m_Spawned;

    [Tooltip("the character being destroyed event")]
    [SerializeField] DisconeCharacterEvent m_Destroyed;

    // -- props --
    /// if the character is simulating
    bool m_IsPerceived;

    /// the underlying character
    Character m_Character;

    /// the music
    CharacterMusic m_Musics;

    /// the dialogue
    CharacterDialogue m_Dialogue;

    /// the checkpoint spawner
    CharacterCheckpoint m_Checkpoint;

    /// the world coordinate
    WorldCoord m_Coord;

    /// the list of simulated children
    GameObject[] m_Simulated;

    /// the time of the last state sync
    [SyncVar]
    double m_LastSync;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Character = GetComponent<Character>();
        m_Coord = GetComponent<WorldCoord>();
        m_Musics = GetComponentInChildren<CharacterMusic>();
        m_Dialogue = GetComponentInChildren<CharacterDialogue>();
        m_Checkpoint = GetComponent<CharacterCheckpoint>();

        // cache list of simulated children -- anything that's active in the prefab
        // TODO: this if for the camera, it's hacky right now
        m_Simulated = Enumerable.Range(0, transform.childCount)
            .Select((i) => transform.GetChild(i).gameObject)
            .Where((c) => c.activeSelf)
            .ToArray();

        // default to not simulating (note, this relies on the above default values being)
        SetSimulation(Simulation.None);

        // set initial coordinate, since we are not simulating
        m_Coord.Value = m_Coord.FromPosition(transform.position);

        // debug
        #if UNITY_EDITOR
        Dbg.AddToParent("Characters", this);
        #endif
    }

    void Start() {
        // send spawned event
        m_Spawned.Raise(this);
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
            var target = m_RemoteState.Copy();
            var delta = (float)(NetworkTime.time - m_LastSync);


            var k = Mathf.Clamp01(delta/m_InterpolationTime);
            var interpolate = CharacterState.Frame.Interpolate(start, target, k);

            // TODO: attempt to also extrapolate...
            // target.Velocity += m_CurrentState.Acceleration * delta;
            // target.Position += target.Velocity * delta;

            m_Character.ForceState(interpolate);
        }
    }

    void OnEnable() {
        SyncSimulation(true);
    }

    void OnDisable() {
        SyncSimulation(false);
    }

    void OnDestroy() {
        OnSimulationChanged = null;

        // send destroyed event
        m_Destroyed.Raise(this);
    }

    // -- l/mirror
    [Server]
    public override void OnStartServer() {
        base.OnStartServer();

        // initially, nobody has authority over any character (except the host client)
        Server_RemoveClientAuthority();
    }

    [Client]
    public override void OnStartClient() {
        base.OnStartClient();

        SyncSimulation(true);
    }

    [Client]
    public override void OnStartAuthority() {
        base.OnStartAuthority();

        // we may move to local simulation if we lose this authority over this character
        SyncSimulation();
    }

    [Client]
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
        if (m_RemoteState.Equals(state)) {
            return;
        }

        // sync the current state frame
        m_RemoteState = state;
        m_LastSync = NetworkTime.time;

        if (hasAuthority) {
            Server_SendState(state, m_LastSync);
        }
    }

    /// update simulation given active state and ownership
    public void SyncSimulation(bool simulate = true) {
        // if the identity has not initialized yet, we should't sync
        // simulate locally if owner, remotely otherwise
        var active = simulate && netIdentity.netId != 0;
        var simulation = (active, netIdentity.IsOwner()) switch {
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
        // TODO: if extrapolating might not need to simulate locally at all
        m_Character.IsPaused = !isSimulated;

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
        m_RemoteState = state;
        m_LastSync = time;

        if (!connectionToClient.observing.Contains(netIdentity)) {
            Debug.Log($"Player cannot observe {name}");
        }

        // place the character in the correct position on the server, since they
        // don't need to interpolate
        m_Character.ForceState(state);
    }

    public void Host_SetVisibility(bool visible) {
        SyncSimulation(visible);
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

    // -- events --
    /// when the perceived state changes
    public delegate void SimulationChangedEvent(Simulation sim);

    public SimulationChangedEvent OnSimulationChanged;

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
    }

    /// release this character
    public void OnRelease() {
    }

    // -- queries --
    /// the character's key
    public CharacterKey Key {
        get => m_Key;
    }

    /// if this character is available
    public bool IsAvailable {
        get => m_IsAvailable;
    }

    /// if the character is selected initially
    public bool IsInitial {
        get => m_IsInitial;
    }

    /// the character's current position
    public Vector3 Position {
        get => m_RemoteState.Position;
    }

    /// if the character is simulating
    public bool IsSimulating {
        get => m_Simulation != Simulation.None;
    }

    /// the third person character
    public ThirdPerson.Character Character {
        get => m_Character;
    }

    /// the music
    public CharacterMusic Music {
        get => m_Musics;
    }

    /// the character dialgue
    public CharacterDialogue Dialogue {
        get => m_Dialogue;
    }

    /// the checkpoint spawner
    public CharacterCheckpoint Checkpoint {
        get => m_Checkpoint;
    }

    /// the character's flower
    public CharacterFlower Flower {
        get => m_Checkpoint.Flower;
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

    // -- factories --
    /// instantiate a rec from a character
    public CharacterRec IntoRecord() {
        return new CharacterRec(
            Key,
            m_RemoteState.Position,
            m_RemoteState.LookRotation,
            m_Checkpoint.Flower?.IntoRecord()
        );
    }
}