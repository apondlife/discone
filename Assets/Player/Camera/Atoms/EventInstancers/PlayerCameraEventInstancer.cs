using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event Instancer of type `PlayerCamera`. Inherits from `AtomEventInstancer&lt;PlayerCamera, PlayerCameraEvent&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-sign-blue")]
    [AddComponentMenu("Unity Atoms/Event Instancers/PlayerCamera Event Instancer")]
    public class PlayerCameraEventInstancer : AtomEventInstancer<PlayerCamera, PlayerCameraEvent> { }
}
