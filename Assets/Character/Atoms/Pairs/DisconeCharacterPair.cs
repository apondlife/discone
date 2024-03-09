using Discone;
using System;
using UnityEngine;

namespace UnityAtoms
{
    /// <summary>
    /// IPair of type `&lt;DisconeCharacter&gt;`. Inherits from `IPair&lt;DisconeCharacter&gt;`.
    /// </summary>
    [Serializable]
    public struct DisconeCharacterPair : IPair<DisconeCharacter>
    {
        public DisconeCharacter Item1 { get => _item1; set => _item1 = value; }
        public DisconeCharacter Item2 { get => _item2; set => _item2 = value; }

        [SerializeField]
        private DisconeCharacter _item1;
        [SerializeField]
        private DisconeCharacter _item2;

        public void Deconstruct(out DisconeCharacter item1, out DisconeCharacter item2) { item1 = Item1; item2 = Item2; }
    }
}