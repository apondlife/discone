#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `DisconePlayer`. Inherits from `AtomDrawer&lt;DisconePlayerEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(DisconePlayerEvent))]
    public class DisconePlayerEventDrawer : AtomDrawer<DisconePlayerEvent> { }
}
#endif
