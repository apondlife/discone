using UnityEngine;
using Discone.Ui;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `MenuPair`. Inherits from `AtomEvent&lt;MenuPair&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/MenuPair", fileName = "MenuPairEvent")]
    public sealed class MenuPairEvent : AtomEvent<MenuPair>
    {
    }
}
