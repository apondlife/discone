#if UNITY_2019_1_OR_NEWER
using UnityEditor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `MenuAction`. Inherits from `AtomDrawer&lt;MenuActionEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(MenuActionEvent))]
    public class MenuActionEventDrawer : AtomDrawer<MenuActionEvent> { }
}
#endif
