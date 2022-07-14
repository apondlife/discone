#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `DisconePlayerPair`. Inherits from `AtomDrawer&lt;DisconePlayerPairEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(DisconePlayerPairEvent))]
    public class DisconePlayerPairEventDrawer : AtomDrawer<DisconePlayerPairEvent> { }
}
#endif
