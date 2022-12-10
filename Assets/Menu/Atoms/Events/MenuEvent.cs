using UnityEngine;
using Discone.Ui;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `Menu`. Inherits from `AtomEvent&lt;Menu&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/Menu", fileName = "MenuEvent")]
    public sealed class MenuEvent : AtomEvent<Menu>
    {
    }
}
