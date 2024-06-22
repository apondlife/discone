#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `Character`. Inherits from `AtomDrawer&lt;CharacterEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(CharacterEvent))]
    public class CharacterEventDrawer : AtomDrawer<CharacterEvent> { }
}
#endif