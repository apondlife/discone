#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Constant property drawer of type `PlayerCamera`. Inherits from `AtomDrawer&lt;PlayerCameraConstant&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(PlayerCameraConstant))]
    public class PlayerCameraConstantDrawer : VariableDrawer<PlayerCameraConstant> { }
}
#endif
