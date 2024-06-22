#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Value List property drawer of type `Character`. Inherits from `AtomDrawer&lt;CharacterValueList&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(CharacterValueList))]
    public class CharacterValueListDrawer : AtomDrawer<CharacterValueList> { }
}
#endif
