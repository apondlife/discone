#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using Discone;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `CharacterFlower`. Inherits from `AtomEventEditor&lt;CharacterFlower, CharacterFlowerEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(CharacterFlowerEvent))]
    public sealed class CharacterFlowerEventEditor : AtomEventEditor<CharacterFlower, CharacterFlowerEvent> { }
}
#endif