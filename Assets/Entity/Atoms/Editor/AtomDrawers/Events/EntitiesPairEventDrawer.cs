#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `EntitiesPair`. Inherits from `AtomDrawer&lt;EntitiesPairEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(EntitiesPairEvent))]
    public class EntitiesPairEventDrawer : AtomDrawer<EntitiesPairEvent> { }
}
#endif
