using System;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event Reference of type `PlayerPair`. Inherits from `AtomEventReference&lt;PlayerPair, PlayerVariable, PlayerPairEvent, PlayerVariableInstancer, PlayerPairEventInstancer&gt;`.
    /// </summary>
    [Serializable]
    public sealed class PlayerPairEventReference : AtomEventReference<
        PlayerPair,
        PlayerVariable,
        PlayerPairEvent,
        PlayerVariableInstancer,
        PlayerPairEventInstancer>, IGetEvent 
    { }
}
