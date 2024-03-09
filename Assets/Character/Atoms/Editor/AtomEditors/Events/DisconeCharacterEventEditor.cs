#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using Discone;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `DisconeCharacter`. Inherits from `AtomEventEditor&lt;DisconeCharacter, DisconeCharacterEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(DisconeCharacterEvent))]
    public sealed class DisconeCharacterEventEditor : AtomEventEditor<DisconeCharacter, DisconeCharacterEvent> { }
}
#endif