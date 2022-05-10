#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;

namespace UnityAtoms.Discone.Editor
{
    /// <summary>
    /// Event property drawer of type `Region`. Inherits from `AtomEventEditor&lt;Region, RegionEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(RegionEvent))]
    public sealed class RegionEventEditor : AtomEventEditor<Region, RegionEvent> { }
}
#endif
