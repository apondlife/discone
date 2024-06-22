using System;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event Reference of type `PlayerCameraPair`. Inherits from `AtomEventReference&lt;PlayerCameraPair, PlayerCameraVariable, PlayerCameraPairEvent, PlayerCameraVariableInstancer, PlayerCameraPairEventInstancer&gt;`.
    /// </summary>
    [Serializable]
    public sealed class PlayerCameraPairEventReference : AtomEventReference<
        PlayerCameraPair,
        PlayerCameraVariable,
        PlayerCameraPairEvent,
        PlayerCameraVariableInstancer,
        PlayerCameraPairEventInstancer>, IGetEvent 
    { }
}
