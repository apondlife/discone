using Discone;
using System;
using UnityEngine;

namespace UnityAtoms
{
    /// <summary>
    /// IPair of type `&lt;DisconeCharacter&gt;`. Inherits from `IPair&lt;DisconeCharacter&gt;`.
    /// </summary>
    [Serializable]
    public struct DisconeCharacterPair : IPair<Character>
    {
        public Character Item1 { get => _item1; set => _item1 = value; }
        public Character Item2 { get => _item2; set => _item2 = value; }

        [SerializeField]
        private Character _item1;
        [SerializeField]
        private Character _item2;

        public void Deconstruct(out Character item1, out Character item2) { item1 = Item1; item2 = Item2; }
    }
}