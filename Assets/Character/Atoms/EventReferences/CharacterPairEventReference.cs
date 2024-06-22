using System;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event Reference of type `CharacterPair`. Inherits from `AtomEventReference&lt;CharacterPair, CharacterVariable, CharacterPairEvent, CharacterVariableInstancer, CharacterPairEventInstancer&gt;`.
    /// </summary>
    [Serializable]
    public sealed class CharacterPairEventReference : AtomEventReference<
        CharacterPair,
        CharacterVariable,
        CharacterPairEvent,
        CharacterVariableInstancer,
        CharacterPairEventInstancer>, IGetEvent 
    { }
}
