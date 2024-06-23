#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `Placement`. Inherits from `AtomDrawer&lt;PlacementEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(PlacementEvent))]
    public class PlacementEventDrawer : AtomDrawer<PlacementEvent> { }
}
#endif
