#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `Menu`. Inherits from `AtomDrawer&lt;MenuEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(MenuEvent))]
    public class MenuEventDrawer : AtomDrawer<MenuEvent> { }
}
#endif
