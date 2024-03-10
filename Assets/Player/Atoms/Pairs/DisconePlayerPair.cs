using System;
using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// IPair of type `&lt;DisconePlayer&gt;`. Inherits from `IPair&lt;DisconePlayer&gt;`.
    /// </summary>
    [Serializable]
    public struct DisconePlayerPair : IPair<Player>
    {
        public Player Item1 { get => _item1; set => _item1 = value; }
        public Player Item2 { get => _item2; set => _item2 = value; }

        [SerializeField]
        private Player _item1;
        [SerializeField]
        private Player _item2;

        public void Deconstruct(out Player item1, out Player item2) { item1 = Item1; item2 = Item2; }
    }
}