using Mirror;
using UnityEngine;

/// extensions to mirro's network identity
static class NetworkIdentityExt {
    /// if this instance (client or server) is the owner of this identity
    public static bool IsOwner(this NetworkIdentity identity) {
        return (
            // if the client has authority
            (identity.isClient && identity.hasAuthority) ||
            // or the server knows no client has a authority
            (identity.isServer && identity.connectionToClient == null)
        );
    }
}