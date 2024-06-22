using System;
using UnityAtoms.BaseAtoms;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Reference of type `Player`. Inherits from `AtomReference&lt;Player, PlayerPair, PlayerConstant, PlayerVariable, PlayerEvent, PlayerPairEvent, PlayerPlayerFunction, PlayerVariableInstancer, AtomCollection, AtomList&gt;`.
    /// </summary>
    [Serializable]
    public sealed class PlayerReference : AtomReference<
        Player,
        PlayerPair,
        PlayerConstant,
        PlayerVariable,
        PlayerEvent,
        PlayerPairEvent,
        PlayerPlayerFunction,
        PlayerVariableInstancer>, IEquatable<PlayerReference>
    {
        public PlayerReference() : base() { }
        public PlayerReference(Player value) : base(value) { }
        public bool Equals(PlayerReference other) { return base.Equals(other); }
        protected override bool ValueEquals(Player other)
        {
            throw new NotImplementedException();
        }
    }
}
