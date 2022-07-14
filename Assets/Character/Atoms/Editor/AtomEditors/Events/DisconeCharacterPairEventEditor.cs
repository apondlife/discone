#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `DisconeCharacterPair`. Inherits from `AtomEventEditor&lt;DisconeCharacterPair, DisconeCharacterPairEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(DisconeCharacterPairEvent))]
    public sealed class DisconeCharacterPairEventEditor : AtomEventEditor<DisconeCharacterPair, DisconeCharacterPairEvent> { }
}
#endif
