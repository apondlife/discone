using System;
using UnityEngine;
using Discone;
namespace UnityAtoms
{
    /// <summary>
    /// IPair of type `&lt;PlayerCamera&gt;`. Inherits from `IPair&lt;PlayerCamera&gt;`.
    /// </summary>
    [Serializable]
    public struct PlayerCameraPair : IPair<PlayerCamera>
    {
        public PlayerCamera Item1 { get => _item1; set => _item1 = value; }
        public PlayerCamera Item2 { get => _item2; set => _item2 = value; }

        [SerializeField]
        private PlayerCamera _item1;
        [SerializeField]
        private PlayerCamera _item2;

        public void Deconstruct(out PlayerCamera item1, out PlayerCamera item2) { item1 = Item1; item2 = Item2; }
    }
}