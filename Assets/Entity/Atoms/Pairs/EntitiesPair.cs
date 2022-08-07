using System;
using UnityEngine;
namespace UnityAtoms
{
    /// <summary>
    /// IPair of type `&lt;Entities&gt;`. Inherits from `IPair&lt;Entities&gt;`.
    /// </summary>
    [Serializable]
    public struct EntitiesPair : IPair<Entities>
    {
        public Entities Item1 { get => _item1; set => _item1 = value; }
        public Entities Item2 { get => _item2; set => _item2 = value; }

        [SerializeField]
        private Entities _item1;
        [SerializeField]
        private Entities _item2;

        public void Deconstruct(out Entities item1, out Entities item2) { item1 = Item1; item2 = Item2; }
    }
}