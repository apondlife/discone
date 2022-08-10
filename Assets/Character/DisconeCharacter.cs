using UnityEngine;
using Mirror;
using ThirdPerson;
using System;

/// an online character
[RequireComponent(typeof(Character))]
[RequireComponent(typeof(CharacterCheckpoint))]
[RequireComponent(typeof(CharacterWrap))]
[RequireComponent(typeof(WorldCoord))]
public sealed class DisconeCharacter: NetworkBehaviour {
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
    [SerializeField] CharacterState.Frame m_CurrentState;

    // -- config --
    [Header("config")]
    [Tooltip("the character's perception")]
    [SerializeField] CharacterPerception m_Perception;

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

    /// whether or not the character is being simulated (not being culled)
    bool m_IsSimulating = true;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Character = GetComponent<Character>();
        m_Checkpoint = GetComponent<CharacterCheckpoint>();
        m_Dialogue = GetComponentInChildren<CharacterDialogue>();
        m_Coord = GetComponent<WorldCoord>();

        // default to not simulating
        OnIsPerceivedChanged();

        // debug
        #if UNITY_EDITOR
        Dbg.AddToParent("Characters", this);
        #endif
    }

    void FixedUpdate() {
        if (m_IsSimulating) {
            SyncState();
        }
    }

    // -- l/mirror
    public override void OnStartServer() {
        base.OnStartServer();

        // initially, nobody has authority over any character (except the host client)
        Server_RemoveClientAuthority();
    }

    // -- commands --
    /// sync state from client -> server, if necessary
    void SyncState() {
        // if we don't have authority, do nothing
        if (!hasAuthority || !isClient) {
            return;
        }

        // if the state did not change, do nothing
        var state = m_Character.CurrentState;
        if (m_CurrentState.Equals(state)) {
            return;
        }

        // sync the current state frame
        m_CurrentState = state;
        Server_SyncState(state);
    }

    // set the character to simluating or not (basically, isActive but w/ network identity)
    public void SetSimulating(bool isSimulating) {
        // ignore redundant calls
        if (m_IsSimulating == isSimulating) {
            return;
        }

        // update state
        m_IsSimulating = isSimulating;

        // toggle pause for the third person simulation
        m_Character.IsPaused = !isSimulating;

        // toggle activity on all the children to turn off rendering, effects, &c
        foreach (Transform c in m_Character.transform) {
            c.gameObject.SetActive(isSimulating);
        }
    }

    // -- c/server
    /// sync this character's current state from the client
    [Command]
    void Server_SyncState(CharacterState.Frame state) {
        m_CurrentState = state;
    }

    /// mark this character as unavaialble; only call on the server
    public void Server_AssignClientAuthority(NetworkConnection connection) {
        m_IsAvailable = false;
        netIdentity.RemoveClientAuthority();
        netIdentity.AssignClientAuthority(connection);
    }

    /// mark this character as available; only call this on the server
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
    void OnIsPerceivedChanged() {
        m_Music.SetActive(m_IsPerceived);
        // TODO: if not host, also stop simulating characters that aren't perceived
    }

    // -- e/client
    void Client_OnStateReceived(CharacterState.Frame src, CharacterState.Frame dst) {
        // ignore state if we have authority
        if (hasAuthority) {
            return;
        }

        // update character's current state frame
        m_Character.ForceState(dst);
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