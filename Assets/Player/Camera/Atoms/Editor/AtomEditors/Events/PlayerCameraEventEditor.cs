#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;
using Discone;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `PlayerCamera`. Inherits from `AtomEventEditor&lt;PlayerCamera, PlayerCameraEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(PlayerCameraEvent))]
    public sealed class PlayerCameraEventEditor : AtomEventEditor<PlayerCamera, PlayerCameraEvent> { }
}
#endif
