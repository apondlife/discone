using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `PlayerCameraPair`. Inherits from `AtomEvent&lt;PlayerCameraPair&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/PlayerCameraPair", fileName = "PlayerCameraPairEvent")]
    public sealed class PlayerCameraPairEvent : AtomEvent<PlayerCameraPair>
    {
    }
}
