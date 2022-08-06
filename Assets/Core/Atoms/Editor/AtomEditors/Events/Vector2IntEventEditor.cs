#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `Vector2Int`. Inherits from `AtomEventEditor&lt;Vector2Int, Vector2IntEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(Vector2IntEvent))]
    public sealed class Vector2IntEventEditor : AtomEventEditor<Vector2Int, Vector2IntEvent> { }
}
#endif
