#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;
using Discone;

namespace UnityAtoms.Discone.Editor
{
    /// <summary>
    /// Event property drawer of type `Region`. Inherits from `AtomDrawer&lt;RegionEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(RegionEvent))]
    public class RegionEventDrawer : AtomDrawer<RegionEvent> { }
}
#endif
