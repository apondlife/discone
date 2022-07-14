#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `DisconeCharacter`. Inherits from `AtomDrawer&lt;DisconeCharacterEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(DisconeCharacterEvent))]
    public class DisconeCharacterEventDrawer : AtomDrawer<DisconeCharacterEvent> { }
}
#endif
