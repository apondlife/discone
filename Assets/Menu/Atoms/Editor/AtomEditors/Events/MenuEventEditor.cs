#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using Menu = Discone.Ui.Menu;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `Menu`. Inherits from `AtomEventEditor&lt;Menu, MenuEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(MenuEvent))]
    public sealed class MenuEventEditor : AtomEventEditor<Menu, MenuEvent> { }
}
#endif
