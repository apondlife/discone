#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Discone.Editor
{
    /// <summary>
    /// Value List property drawer of type `Region`. Inherits from `AtomDrawer&lt;RegionValueList&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(RegionValueList))]
    public class RegionValueListDrawer : AtomDrawer<RegionValueList> { }
}
#endif
