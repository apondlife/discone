#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Variable property drawer of type `Menu`. Inherits from `AtomDrawer&lt;MenuVariable&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(MenuVariable))]
    public class MenuVariableDrawer : VariableDrawer<MenuVariable> { }
}
#endif
