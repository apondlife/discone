#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Constant property drawer of type `Player`. Inherits from `AtomDrawer&lt;PlayerConstant&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(PlayerConstant))]
    public class PlayerConstantDrawer : VariableDrawer<PlayerConstant> { }
}
#endif
