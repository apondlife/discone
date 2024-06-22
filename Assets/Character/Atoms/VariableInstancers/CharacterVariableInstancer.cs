using UnityEngine;
using UnityAtoms.BaseAtoms;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Variable Instancer of type `Character`. Inherits from `AtomVariableInstancer&lt;CharacterVariable, CharacterPair, Character, CharacterEvent, CharacterPairEvent, CharacterCharacterFunction&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-hotpink")]
    [AddComponentMenu("Unity Atoms/Variable Instancers/Character Variable Instancer")]
    public class CharacterVariableInstancer : AtomVariableInstancer<
        CharacterVariable,
        CharacterPair,
        Character,
        CharacterEvent,
        CharacterPairEvent,
        CharacterCharacterFunction>
    { }
}
