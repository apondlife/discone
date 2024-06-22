using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event Instancer of type `PlayerCameraPair`. Inherits from `AtomEventInstancer&lt;PlayerCameraPair, PlayerCameraPairEvent&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-sign-blue")]
    [AddComponentMenu("Unity Atoms/Event Instancers/PlayerCameraPair Event Instancer")]
    public class PlayerCameraPairEventInstancer : AtomEventInstancer<PlayerCameraPair, PlayerCameraPairEvent> { }
}
