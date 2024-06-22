using System;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event Reference of type `Character`. Inherits from `AtomEventReference&lt;Character, CharacterVariable, CharacterEvent, CharacterVariableInstancer, CharacterEventInstancer&gt;`.
    /// </summary>
    [Serializable]
    public sealed class CharacterEventReference : AtomEventReference<
        Character,
        CharacterVariable,
        CharacterEvent,
        CharacterVariableInstancer,
        CharacterEventInstancer>, IGetEvent 
    { }
}
