using UnityEngine;
using UnityAtoms.BaseAtoms;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Set variable value Action of type `Character`. Inherits from `SetVariableValue&lt;Character, CharacterPair, CharacterVariable, CharacterConstant, CharacterReference, CharacterEvent, CharacterPairEvent, CharacterVariableInstancer&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-purple")]
    [CreateAssetMenu(menuName = "Unity Atoms/Actions/Set Variable Value/Character", fileName = "SetCharacterVariableValue")]
    public sealed class SetCharacterVariableValue : SetVariableValue<
        Character,
        CharacterPair,
        CharacterVariable,
        CharacterConstant,
        CharacterReference,
        CharacterEvent,
        CharacterPairEvent,
        CharacterCharacterFunction,
        CharacterVariableInstancer>
    { }
}
