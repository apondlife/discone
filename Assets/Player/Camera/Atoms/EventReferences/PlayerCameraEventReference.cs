using System;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event Reference of type `PlayerCamera`. Inherits from `AtomEventReference&lt;PlayerCamera, PlayerCameraVariable, PlayerCameraEvent, PlayerCameraVariableInstancer, PlayerCameraEventInstancer&gt;`.
    /// </summary>
    [Serializable]
    public sealed class PlayerCameraEventReference : AtomEventReference<
        PlayerCamera,
        PlayerCameraVariable,
        PlayerCameraEvent,
        PlayerCameraVariableInstancer,
        PlayerCameraEventInstancer>, IGetEvent 
    { }
}
