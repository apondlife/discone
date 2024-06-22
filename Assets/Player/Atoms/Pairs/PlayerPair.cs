using System;
using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// IPair of type `&lt;Player&gt;`. Inherits from `IPair&lt;Player&gt;`.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "UnityAtoms", "DisconePlayerPair")]
    public struct PlayerPair : IPair<Player>
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