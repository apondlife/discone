#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;
using Discone;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `Placement`. Inherits from `AtomEventEditor&lt;Placement, PlacementEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(PlacementEvent))]
    public sealed class PlacementEventEditor : AtomEventEditor<Placement, PlacementEvent> { }
}
#endif
