#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `PlayerCamera`. Inherits from `AtomDrawer&lt;PlayerCameraEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(PlayerCameraEvent))]
    public class PlayerCameraEventDrawer : AtomDrawer<PlayerCameraEvent> { }
}
#endif
