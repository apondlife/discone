#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `Vector2Int`. Inherits from `AtomDrawer&lt;Vector2IntEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(Vector2IntEvent))]
    public class Vector2IntEventDrawer : AtomDrawer<Vector2IntEvent> { }
}
#endif
