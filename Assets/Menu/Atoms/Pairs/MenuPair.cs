using System;
using UnityEngine;
using Discone.Ui;
namespace UnityAtoms
{
    /// <summary>
    /// IPair of type `&lt;Menu&gt;`. Inherits from `IPair&lt;Menu&gt;`.
    /// </summary>
    [Serializable]
    public struct MenuPair : IPair<Menu>
    {
        public Menu Item1 { get => _item1; set => _item1 = value; }
        public Menu Item2 { get => _item2; set => _item2 = value; }

        [SerializeField]
        private Menu _item1;
        [SerializeField]
        private Menu _item2;

        public void Deconstruct(out Menu item1, out Menu item2) { item1 = Item1; item2 = Item2; }
    }
}