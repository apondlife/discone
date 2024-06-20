using System;
using Mirror;
using System.Linq;
using ThirdPerson;
using UnityEngine;

namespace Discone {

// TODO: only synchronize properties required for presentation, not simulation. here's a
// list of what's used as of the time of this comment:
// - Position
// - Velocity
// - Inertia
// - Force
// - Acceleration
// - Forward
// - MainSurface
// - Surfaces
// - JumpState
// - NextJump
// - IsInJumpSquat
// - CoyoteTime
// - IdleTime
// - Tilt
// - Events

/// an online character
[RequireComponent(typeof(Character))]
public sealed class Character_Online: NetworkBehaviour {
    // -- state --
    [Header("state")]
    [Tooltip("where the simulation for this character takes place")]
    [SerializeField] CharacterSimulation m_Simulation = CharacterSimulation.None;

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
    [SerializeField] CharacterState.Frame m_RemoteState;

    // -- cfg --
    [Header("cfg")]
    [Tooltip("how long does the character take to interpolate to the current received state")]
    [SerializeField] float m_InterpolationTime = 0.2f;

    // -- props --
    /// the underlying character
    Character m_Character;

    /// the list of simulated children
    GameObject[] m_Simulated;

    /// the time of the last state sync
    [SyncVar]
    double m_LastSync;

    /// the interpolated character state frame;
    CharacterState.Frame m_InterpolatedState;

    public Action<CharacterSimulation> OnSimulationChanged;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Character = GetComponent<Character>();

        // cache list of simulated children -- anything that's active in the prefab
        // TODO: this is for the camera, it's hacky right now
        m_Simulated = Enumerable.Range(0, transform.childCount)
            .Select((i) => transform.GetChild(i).gameObject)
            .Where((c) => c.activeSelf)
            .ToArray();

        // default to not simulating (note, this relies on the above default values being)
        SetSimulation(CharacterSimulation.None);
    }

    void FixedUpdate() {
        // if we simulate this character, send its state to all clients
        if (m_Simulation == CharacterSimulation.Local) {
            SendState();
        }
        // otherwise, if the simulation is remote and we're a client, interpolate
        // state to smooth out gaps
        else if (isClient && m_InterpolatedState != null) {
            var src = m_Character.State.Next;
            var dst = m_RemoteState;

            var delta = (float)(NetworkTime.time - m_LastSync);
            var k = Mathf.Clamp01(delta / m_InterpolationTime);

            // TODO: attempt to also extrapolate...
            // target.Velocity += m_CurrentState.Acceleration * delta;
            // target.Position += target.Velocity * delta;
            CharacterState.Frame.Interpolate(
                src,
                dst,
                ref m_InterpolatedState,
                k
            );

            m_Character.ForceState(m_InterpolatedState);
        }
    }

    void OnEnable() {
        SyncSimulation(true);
    }

    void OnDisable() {
        SyncSimulation(false);
    }

    // -- l/mirror
    /// [Server]
    public override void OnStartServer() {
        base.OnStartServer();

        // initially, nobody has authority over any character (except the host client)
        Server_RemoveClientAuthority();
    }

    /// [Client]
    public override void OnStartClient() {
        base.OnStartClient();

        SyncSimulation(true);
    }

    /// [Client]
    public override void OnStartAuthority() {
        base.OnStartAuthority();

        // we may move to local simulation if we lose this authority over this character
        SyncSimulation();
    }

    /// [Client]
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

        if (isOwned) {
            Server_SendState(state, m_LastSync);
        }
    }

    /// update simulation given active state and ownership
    public void SyncSimulation(bool simulate = true) {
        // if the identity has not initialized yet, we should't sync
        // simulate locally if owner, remotely otherwise
        var active = simulate && netIdentity.netId != 0;
        var simulation = (active, netIdentity.IsOwner()) switch {
            (false, _) => CharacterSimulation.None,
            (_, true) => CharacterSimulation.Local,
            (_, false) => CharacterSimulation.Remote,
        };

        // only set if changed
        if (m_Simulation != simulation) {
            SetSimulation(simulation);
        }
    }

    /// force the simulation state; this has a bunch of side-effects, call it cautiously
    void SetSimulation(CharacterSimulation simulation) {
        // update state
        m_Simulation = simulation;

        // if the character is simulated at all
        var isSimulated = simulation != CharacterSimulation.None;

        // pause when not simulated at all
        // TODO: if extrapolating might not need to simulate locally at all
        if (!isSimulated) {
            m_Character.Pause();
        } else {
            m_Character.Unpause();
        }

        // toggle activity on all the children to turn off rendering, effects, &c
        foreach (var c in m_Simulated) {
            c.SetActive(isSimulated);
        }

        // if not remote any more, clear interpolated state
        if (simulation != CharacterSimulation.Remote) {
            m_InterpolatedState = null;
        }

        OnSimulationChanged?.Invoke(simulation);
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

    /// mark this character as unavailable; only call on the server
    [Server]
    public void Server_AssignClientAuthority(NetworkConnectionToClient connection) {
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
    /// when the client receives new state from the server
    [Client]
    void Client_OnStateReceived(CharacterState.Frame prev, CharacterState.Frame next) {
        if (m_Simulation != CharacterSimulation.Remote) {
            return;
        }

        // if interpolating, save the a copy of the target state
        if (m_InterpolationTime > 0.0f) {
            m_InterpolatedState = next.Copy();
        }
        // otherwise, just update to whatever the server sends
        else {
            m_Character.ForceState(next);
        }
    }

    // -- queries --
    /// the Character
    public Character Character {
        get => m_Character;
    }

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
        get => m_Simulation != CharacterSimulation.None;
    }

    // -- q/debug
    #if UNITY_EDITOR
    /// if this is the debug character
    public bool IsDebug {
        get => m_IsDebug;
    }
    #endif
}

}