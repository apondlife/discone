using UnityEngine;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `Entities`. Inherits from `AtomEvent&lt;Entities&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/Entities", fileName = "EntitiesEvent")]
    public sealed class EntitiesEvent : AtomEvent<Entities>
    {
    }
}
