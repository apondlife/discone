using UnityEngine;
using Discone.Ui;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `MenuAction`. Inherits from `AtomEvent&lt;MenuAction&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/MenuAction", fileName = "MenuActionEvent")]
    public sealed class MenuActionEvent : AtomEvent<MenuAction>
    {
    }
}
