using Discone;
using UnityEngine;

namespace UnityAtoms
{
    /// <summary>
    /// Variable of type `Character`. Inherits from `AtomVariable&lt;Character, CharacterPair, CharacterEvent, CharacterPairEvent, CharacterCharacterFunction&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-lush")]
    [CreateAssetMenu(menuName = "Unity Atoms/Variables/Character", fileName = "CharacterVariable")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "UnityAtoms", "DisconeCharacterVariable")]
    public sealed class CharacterVariable : AtomVariable<Character, CharacterPair, CharacterEvent, CharacterPairEvent, CharacterCharacterFunction>
    {
        protected override bool ValueEquals(Character other)
        {
            return Value == other;
        }
    }
}