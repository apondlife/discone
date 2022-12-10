using UnityEngine;
using Discone.Ui;

namespace UnityAtoms
{
    /// <summary>
    /// Variable of type `Menu`. Inherits from `EquatableAtomVariable&lt;Menu, MenuPair, MenuEvent, MenuPairEvent, MenuMenuFunction&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-lush")]
    [CreateAssetMenu(menuName = "Unity Atoms/Variables/Menu", fileName = "MenuVariable")]
    public sealed class MenuVariable : AtomVariable<Menu, MenuPair, MenuEvent, MenuPairEvent, MenuMenuFunction> {
        protected override bool ValueEquals(Menu other) {
            return System.Object.ReferenceEquals(other, Value);
        }
    }
}
