#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `Entities`. Inherits from `AtomEventEditor&lt;Entities, EntitiesEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(EntitiesEvent))]
    public sealed class EntitiesEventEditor : AtomEventEditor<Entities, EntitiesEvent> { }
}
#endif
