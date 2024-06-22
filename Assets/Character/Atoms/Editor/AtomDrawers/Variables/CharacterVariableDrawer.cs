#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Variable property drawer of type `Character`. Inherits from `AtomDrawer&lt;CharacterVariable&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(CharacterVariable))]
    public class CharacterVariableDrawer : VariableDrawer<CharacterVariable> { }
}
#endif