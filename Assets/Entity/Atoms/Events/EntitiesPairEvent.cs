using UnityEngine;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `EntitiesPair`. Inherits from `AtomEvent&lt;EntitiesPair&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/EntitiesPair", fileName = "EntitiesPairEvent")]
    public sealed class EntitiesPairEvent : AtomEvent<EntitiesPair>
    {
    }
}
