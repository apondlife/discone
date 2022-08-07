#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `World`. Inherits from `AtomEventEditor&lt;World, WorldEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(WorldEvent))]
    public sealed class WorldEventEditor : AtomEventEditor<World, WorldEvent> { }
}
#endif
