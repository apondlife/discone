using UnityEngine;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `DisconePlayer`. Inherits from `AtomEvent&lt;DisconePlayer&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/DisconePlayer", fileName = "DisconePlayerEvent")]
    public sealed class DisconePlayerEvent : AtomEvent<DisconePlayer>
    {
    }
}
