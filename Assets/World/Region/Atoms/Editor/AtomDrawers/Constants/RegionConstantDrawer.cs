#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;
using Discone;

namespace UnityAtoms.Discone.Editor
{
    /// <summary>
    /// Constant property drawer of type `Region`. Inherits from `AtomDrawer&lt;RegionConstant&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(RegionConstant))]
    public class RegionConstantDrawer : VariableDrawer<RegionConstant> { }
}
#endif
