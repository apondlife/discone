using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `Player`. Inherits from `AtomEvent&lt;Player&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/Player", fileName = "PlayerEvent")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "UnityAtoms", "DisconePlayerEvent")]
    public sealed class PlayerEvent : AtomEvent<Player>
    {
    }
}
