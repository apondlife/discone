#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Constant property drawer of type `Character`. Inherits from `AtomDrawer&lt;CharacterConstant&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(CharacterConstant))]
    public class CharacterConstantDrawer : VariableDrawer<CharacterConstant> { }
}
#endif
