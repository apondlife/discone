using UnityEngine;
using UnityAtoms.BaseAtoms;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Variable Instancer of type `PlayerCamera`. Inherits from `AtomVariableInstancer&lt;PlayerCameraVariable, PlayerCameraPair, PlayerCamera, PlayerCameraEvent, PlayerCameraPairEvent, PlayerCameraPlayerCameraFunction&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-hotpink")]
    [AddComponentMenu("Unity Atoms/Variable Instancers/PlayerCamera Variable Instancer")]
    public class PlayerCameraVariableInstancer : AtomVariableInstancer<
        PlayerCameraVariable,
        PlayerCameraPair,
        PlayerCamera,
        PlayerCameraEvent,
        PlayerCameraPairEvent,
        PlayerCameraPlayerCameraFunction>
    { }
}
