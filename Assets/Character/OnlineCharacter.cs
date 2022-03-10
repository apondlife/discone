using UnityEngine;
using Mirror;

/// an online character
sealed class OnlineCharacter: NetworkBehaviour {
    // -- fields --
    /// if this character is available
    [Header("fields")]
    [SyncVar] [SerializeField] bool m_IsAvailable = true;

    // -- commands --
    /// mark this character as unavaialble
    public void AssignClientAuthority(NetworkConnection connection) {
        m_IsAvailable = false;
        netIdentity.AssignClientAuthority(connection);
    }

    /// mark this character as available
    public void RemoveClientAuthority() {
        m_IsAvailable = true;
        netIdentity.RemoveClientAuthority();
    }

    // -- queries --
    /// if this character is available
    public bool IsAvailable {
        get => m_IsAvailable;
    }
}