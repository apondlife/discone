#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `DisconeCharacterPair`. Inherits from `AtomDrawer&lt;DisconeCharacterPairEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(DisconeCharacterPairEvent))]
    public class DisconeCharacterPairEventDrawer : AtomDrawer<DisconeCharacterPairEvent> { }
}
#endif
