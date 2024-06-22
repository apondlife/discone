#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;
using Discone;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `Player`. Inherits from `AtomEventEditor&lt;Player, PlayerEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(PlayerEvent))]
    public sealed class PlayerEventEditor : AtomEventEditor<Player, PlayerEvent> { }
}
#endif
