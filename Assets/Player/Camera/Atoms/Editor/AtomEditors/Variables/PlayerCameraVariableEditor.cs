using UnityEditor;
using UnityAtoms.Editor;
using Discone;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Variable Inspector of type `PlayerCamera`. Inherits from `AtomVariableEditor`
    /// </summary>
    [CustomEditor(typeof(PlayerCameraVariable))]
    public sealed class PlayerCameraVariableEditor : AtomVariableEditor<PlayerCamera, PlayerCameraPair> { }
}
