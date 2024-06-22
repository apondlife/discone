#if UNITY_2019_1_OR_NEWER
using UnityEditor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `CharacterPair`. Inherits from `AtomEventEditor&lt;CharacterPair, CharacterPairEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(CharacterPairEvent))]
    public sealed class CharacterPairEventEditor : AtomEventEditor<CharacterPair, CharacterPairEvent> { }
}
#endif