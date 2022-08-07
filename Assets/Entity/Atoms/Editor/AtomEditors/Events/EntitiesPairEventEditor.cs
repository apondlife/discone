#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `EntitiesPair`. Inherits from `AtomEventEditor&lt;EntitiesPair, EntitiesPairEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(EntitiesPairEvent))]
    public sealed class EntitiesPairEventEditor : AtomEventEditor<EntitiesPair, EntitiesPairEvent> { }
}
#endif
