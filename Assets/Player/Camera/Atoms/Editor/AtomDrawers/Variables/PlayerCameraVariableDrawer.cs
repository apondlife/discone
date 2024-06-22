#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Variable property drawer of type `PlayerCamera`. Inherits from `AtomDrawer&lt;PlayerCameraVariable&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(PlayerCameraVariable))]
    public class PlayerCameraVariableDrawer : VariableDrawer<PlayerCameraVariable> { }
}
#endif
