#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `CharacterPair`. Inherits from `AtomDrawer&lt;CharacterPairEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(CharacterPairEvent))]
    public class CharacterPairEventDrawer : AtomDrawer<CharacterPairEvent> { }
}
#endif