#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `MenuPair`. Inherits from `AtomDrawer&lt;MenuPairEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(MenuPairEvent))]
    public class MenuPairEventDrawer : AtomDrawer<MenuPairEvent> { }
}
#endif
