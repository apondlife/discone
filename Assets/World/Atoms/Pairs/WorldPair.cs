using System;
using UnityEngine;
namespace UnityAtoms
{
    /// <summary>
    /// IPair of type `&lt;World&gt;`. Inherits from `IPair&lt;World&gt;`.
    /// </summary>
    [Serializable]
    public struct WorldPair : IPair<World>
    {
        public World Item1 { get => _item1; set => _item1 = value; }
        public World Item2 { get => _item2; set => _item2 = value; }

        [SerializeField]
        private World _item1;
        [SerializeField]
        private World _item2;

        public void Deconstruct(out World item1, out World item2) { item1 = Item1; item2 = Item2; }
    }
}