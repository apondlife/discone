#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using Discone;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `Character`. Inherits from `AtomEventEditor&lt;Character, CharacterEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(CharacterEvent))]
    public sealed class CharacterEventEditor : AtomEventEditor<Character, CharacterEvent> { }
}
#endif