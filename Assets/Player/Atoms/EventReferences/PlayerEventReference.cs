using System;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event Reference of type `Player`. Inherits from `AtomEventReference&lt;Player, PlayerVariable, PlayerEvent, PlayerVariableInstancer, PlayerEventInstancer&gt;`.
    /// </summary>
    [Serializable]
    public sealed class PlayerEventReference : AtomEventReference<
        Player,
        PlayerVariable,
        PlayerEvent,
        PlayerVariableInstancer,
        PlayerEventInstancer>, IGetEvent 
    { }
}
