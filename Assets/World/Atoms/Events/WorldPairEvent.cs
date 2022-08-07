using UnityEngine;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `WorldPair`. Inherits from `AtomEvent&lt;WorldPair&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/WorldPair", fileName = "WorldPairEvent")]
    public sealed class WorldPairEvent : AtomEvent<WorldPair>
    {
    }
}
