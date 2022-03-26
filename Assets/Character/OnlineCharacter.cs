using UnityEngine;
using Mirror;

/// an online character
sealed class OnlineCharacter: NetworkBehaviour {
    // -- constants --
    static Transform k_Characters;

    // -- fields --
    /// if this character is available
    [Header("fields")]
    [Tooltip("if the character is can be selected initially")]
    [SerializeField] bool m_IsInitial;

    [Tooltip("if the character is currently available")]
    [SyncVar] [SerializeField] bool m_IsAvailable = true;

    // -- lifecycle --
    void Awake() {
        #if UNITY_EDITOR
        // create shared characters "folder"
        if (k_Characters == null) {
            var obj = new GameObject();
            obj.name = "Characters";
            k_Characters = obj.transform;
        }

        // move character to folder
        transform.parent = k_Characters.transform;
        #endif
    }

    public override void OnStartServer() {
        base.OnStartServer();

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