using UnityAtoms.BaseAtoms;
using UnityEngine;

/// processes collisions (perception) for all players and characters
sealed class EntityCollisions: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("if the player is the host")]
    [SerializeField] BoolReference m_IsHost;

    [Tooltip("the world chunk size")]
    [SerializeField] FloatReference m_ChunkSize;
}
