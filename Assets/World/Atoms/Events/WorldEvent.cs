using UnityEngine;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `World`. Inherits from `AtomEvent&lt;World&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/World", fileName = "WorldEvent")]
    public sealed class WorldEvent : AtomEvent<World>
    {
    }
}
