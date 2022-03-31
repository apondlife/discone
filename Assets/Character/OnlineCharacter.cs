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
    [SyncVar] [SerializeField] bool m_IsAvailable = true;

    // -- props --
    /// the underlying character
    Character m_Character;

    /// the character's most recent state frame
    [SyncVar] CharacterState.Frame m_CurrentState;

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
        // if we have authority, push state
        if (netIdentity.hasAuthority) {
            Debug.Log($"[online] char {name} - push state: {!m_Character.CurrentState.Equals(m_CurrentState)}");
            m_CurrentState = m_Character.CurrentState;
        }
        // otherwise, apply whatever was synced
        // NOTE: we guard null b/c the first frame FixedUpdate is called, after OnStartServer &
        // RemoveClientAuthority are called, netIdentity.hasAuthority is false and m_CurrentState
        // is null. there's probably a less hacky way to detect this state.
        else if (m_CurrentState != null) {
            Debug.Log($"[online] char {name} - receive state");
            m_Character.ForceState(m_CurrentState);
        }
    }

    // -- l/mirror
    public override void OnStartServer() {
        base.OnStartServer();

        Debug.Log($"start server");
        // initially, nobody has authority over any character (except the host client)
        RemoveClientAuthority();
    }

    // -- commands --
    /// mark this character as unavaialble
    public void AssignClientAuthority(NetworkConnection connection) {
        m_IsAvailable = false;
        netIdentity.RemoveClientAuthority();
        netIdentity.AssignClientAuthority(connection);
    }

    /// mark this character as available
    public void RemoveClientAuthority() {
        m_IsAvailable = true;
        netIdentity.RemoveClientAuthority();
        netIdentity.AssignClientAuthority(NetworkServer.localConnection);
    }

    // -- queries --
    /// if this character is available
    public bool IsAvailable {
        get => m_IsAvailable;
    }

    // if the character is can be selected initially
    public bool IsInitial {
        get => m_IsInitial;
    }
}