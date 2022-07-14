#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Variable property drawer of type `DisconePlayer`. Inherits from `AtomDrawer&lt;DisconePlayerVariable&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(DisconePlayerVariable))]
    public class DisconePlayerVariableDrawer : VariableDrawer<DisconePlayerVariable> { }
}
#endif
