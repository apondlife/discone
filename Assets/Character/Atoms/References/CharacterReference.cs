using System;
using UnityAtoms.BaseAtoms;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Reference of type `Character`. Inherits from `EquatableAtomReference&lt;Character, CharacterPair, CharacterConstant, CharacterVariable, CharacterEvent, CharacterPairEvent, CharacterCharacterFunction, CharacterVariableInstancer, AtomCollection, AtomList&gt;`.
    /// </summary>
    [Serializable]
    public sealed class CharacterReference : EquatableAtomReference<
        Character,
        CharacterPair,
        CharacterConstant,
        CharacterVariable,
        CharacterEvent,
        CharacterPairEvent,
        CharacterCharacterFunction,
        CharacterVariableInstancer>, IEquatable<CharacterReference>
    {
        public CharacterReference() : base() { }
        public CharacterReference(Character value) : base(value) { }
        public bool Equals(CharacterReference other) { return base.Equals(other); }
    }
}
