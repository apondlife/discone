using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `PlayerPair`. Inherits from `AtomEvent&lt;PlayerPair&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/PlayerPair", fileName = "PlayerPairEvent")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "UnityAtoms", "DisconePlayerPairEvent")]
    public sealed class PlayerPairEvent : AtomEvent<PlayerPair>
    {
    }
}
