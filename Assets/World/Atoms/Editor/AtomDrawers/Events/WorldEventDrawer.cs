#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `World`. Inherits from `AtomDrawer&lt;WorldEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(WorldEvent))]
    public class WorldEventDrawer : AtomDrawer<WorldEvent> { }
}
#endif
