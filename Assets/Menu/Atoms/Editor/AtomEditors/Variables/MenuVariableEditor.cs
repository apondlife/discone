using UnityEditor;
using Menu = Discone.Ui.Menu;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Variable Inspector of type `Menu`. Inherits from `AtomVariableEditor`
    /// </summary>
    [CustomEditor(typeof(MenuVariable))]
    public sealed class MenuVariableEditor : AtomVariableEditor<Menu, MenuPair> { }
}
