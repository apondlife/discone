#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Variable property drawer of type `Player`. Inherits from `AtomDrawer&lt;PlayerVariable&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(PlayerVariable))]
    public class PlayerVariableDrawer : VariableDrawer<PlayerVariable> { }
}
#endif
