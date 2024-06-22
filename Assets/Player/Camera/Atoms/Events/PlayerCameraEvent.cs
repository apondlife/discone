using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `PlayerCamera`. Inherits from `AtomEvent&lt;PlayerCamera&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/PlayerCamera", fileName = "PlayerCameraEvent")]
    public sealed class PlayerCameraEvent : AtomEvent<PlayerCamera>
    {
    }
}
