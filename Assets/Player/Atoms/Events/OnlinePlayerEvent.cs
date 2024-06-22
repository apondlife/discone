using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `OnlinePlayer`. Inherits from `AtomEvent&lt;OnlinePlayer&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/OnlinePlayer", fileName = "OnlinePlayerEvent")]
    public sealed class OnlinePlayerEvent : AtomEvent<OnlinePlayer>
    {
    }
}