using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `DisconePlayer`. Inherits from `AtomEvent&lt;DisconePlayer&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/OnlinePlayer", fileName = "OnlinePlayerEvent")]
    public sealed class OnlinePlayerEvent : AtomEvent<OnlinePlayer>
    {
    }
}
