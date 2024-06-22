#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `Player`. Inherits from `AtomDrawer&lt;PlayerEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(PlayerEvent))]
    public class PlayerEventDrawer : AtomDrawer<PlayerEvent> { }
}
#endif
