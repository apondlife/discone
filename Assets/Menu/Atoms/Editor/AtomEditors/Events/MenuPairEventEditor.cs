#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;
using Discone.Ui;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `MenuPair`. Inherits from `AtomEventEditor&lt;MenuPair, MenuPairEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(MenuPairEvent))]
    public sealed class MenuPairEventEditor : AtomEventEditor<MenuPair, MenuPairEvent> { }
}
#endif
