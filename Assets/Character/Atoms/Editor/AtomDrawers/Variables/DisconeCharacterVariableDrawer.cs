#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Variable property drawer of type `DisconeCharacter`. Inherits from `AtomDrawer&lt;DisconeCharacterVariable&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(DisconeCharacterVariable))]
    public class DisconeCharacterVariableDrawer : VariableDrawer<DisconeCharacterVariable> { }
}
#endif
