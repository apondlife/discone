using System;
using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// IPair of type `&lt;DisconePlayer&gt;`. Inherits from `IPair&lt;DisconePlayer&gt;`.
    /// </summary>
    [Serializable]
    public struct DisconePlayerPair : IPair<DisconePlayer>
    {
        public DisconePlayer Item1 { get => _item1; set => _item1 = value; }
        public DisconePlayer Item2 { get => _item2; set => _item2 = value; }

        [SerializeField]
        private DisconePlayer _item1;
        [SerializeField]
        private DisconePlayer _item2;

        public void Deconstruct(out DisconePlayer item1, out DisconePlayer item2) { item1 = Item1; item2 = Item2; }
    }
}