#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;
using Discone;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `DisconePlayer`. Inherits from `AtomEventEditor&lt;DisconePlayer, DisconePlayerEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(DisconePlayerEvent))]
    public sealed class DisconePlayerEventEditor : AtomEventEditor<Player, DisconePlayerEvent> { }
}
#endif
