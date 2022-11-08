#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `CharacterFlower`. Inherits from `AtomDrawer&lt;CharacterFlowerEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(CharacterFlowerEvent))]
    public class CharacterFlowerEventDrawer : AtomDrawer<CharacterFlowerEvent> { }
}
#endif
