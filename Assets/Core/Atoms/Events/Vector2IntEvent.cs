using UnityEngine;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `Vector2Int`. Inherits from `AtomEvent&lt;Vector2Int&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/Vector2Int", fileName = "Vector2IntEvent")]
    public sealed class Vector2IntEvent : AtomEvent<Vector2Int>
    {
    }
}
