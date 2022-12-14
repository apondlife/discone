#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;
using Discone.Ui;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `MenuAction`. Inherits from `AtomEventEditor&lt;MenuAction, MenuActionEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(MenuActionEvent))]
    public sealed class MenuActionEventEditor : AtomEventEditor<MenuAction, MenuActionEvent> { }
}
#endif
