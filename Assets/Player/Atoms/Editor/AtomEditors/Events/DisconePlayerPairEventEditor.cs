#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `DisconePlayerPair`. Inherits from `AtomEventEditor&lt;DisconePlayerPair, DisconePlayerPairEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(DisconePlayerPairEvent))]
    public sealed class DisconePlayerPairEventEditor : AtomEventEditor<DisconePlayerPair, DisconePlayerPairEvent> { }
}
#endif
