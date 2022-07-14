using UnityEngine;
using System;

namespace UnityAtoms
{
    /// <summary>
    /// Variable of type `DisconeCharacter`. Inherits from `AtomVariable&lt;DisconeCharacter, DisconeCharacterPair, DisconeCharacterEvent, DisconeCharacterPairEvent, DisconeCharacterDisconeCharacterFunction&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-lush")]
    [CreateAssetMenu(menuName = "Unity Atoms/Variables/DisconeCharacter", fileName = "DisconeCharacterVariable")]
    public sealed class DisconeCharacterVariable : AtomVariable<DisconeCharacter, DisconeCharacterPair, DisconeCharacterEvent, DisconeCharacterPairEvent, DisconeCharacterDisconeCharacterFunction>
    {
        protected override bool ValueEquals(DisconeCharacter other)
        {
            return Value == other;
        }
    }
}
