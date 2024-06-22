#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `PlayerCameraPair`. Inherits from `AtomDrawer&lt;PlayerCameraPairEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(PlayerCameraPairEvent))]
    public class PlayerCameraPairEventDrawer : AtomDrawer<PlayerCameraPairEvent> { }
}
#endif
