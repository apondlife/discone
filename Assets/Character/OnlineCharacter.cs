using UnityEngine;
using Mirror;
using ThirdPerson;

/// an online character
[RequireComponent(typeof(Character))]
sealed class OnlineCharacter: NetworkBehaviour {
    // -- constants --
    /// a parent "folder" for the characters
    static Transform k_Characters;

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

    // -- props --
    /// the underlying character
    Character m_Character;

    // -- lifecycle --
    void Awake() {
        // set deps
        m_Character = GetComponent<Character>();

        // debug
        #if UNITY_EDITOR
        // create shared characters go
        if (k_Characters == null) {
            var obj = new GameObject();
            obj.name = "Characters";
            k_Characters = obj.transform;
        }

        // move character to parent
        transform.parent = k_Characters.transform;
        #endif
    }

    void FixedUpdate() {
        // if we have authority
        if (hasAuthority && isClient) {
            // and the state changed
            var state = m_Character.CurrentState;
            if (!m_CurrentState.Equals(state)) {
                // push state
                m_CurrentState = state;
                Server_SyncState(state);
            }
        }
    }

    // -- l/mirror
    public override void OnStartServer() {
        base.OnStartServer();

        // initially, nobody has authority over any character (except the host client)
        Server_RemoveClientAuthority();
    }

    // -- commands --
    /// mark this character as unavaialble
    public void Server_AssignClientAuthority(NetworkConnection connection) {
        m_IsAvailable = false;
        netIdentity.RemoveClientAuthority();
        netIdentity.AssignClientAuthority(connection);
    }

    /// mark this character as available
    public void Server_RemoveClientAuthority() {
        m_IsAvailable = true;
        netIdentity.RemoveClientAuthority();
        netIdentity.AssignClientAuthority(NetworkServer.localConnection);
    }

    /// sync this character's current state from the client
    [Command]
    void Server_SyncState(CharacterState.Frame state) {
        m_CurrentState = state;
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

    // -- q/debug
    #if UNITY_EDITOR
    /// if this is the debug character
    public bool IsDebug {
        get => m_IsDebug;
    }
    #endif

    // -- events --
    void Client_OnStateReceived(CharacterState.Frame src, CharacterState.Frame dst) {
        Debug.Log($"{name} Received some state");
        // ignore state if we have authority
        if (hasAuthority) {
            return;
        }

        // update character's current state frame
        Debug.Log($"{name} Forcing state for");
        m_Character.ForceState(dst);
    }
}