#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `WorldPair`. Inherits from `AtomEventEditor&lt;WorldPair, WorldPairEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(WorldPairEvent))]
    public sealed class WorldPairEventEditor : AtomEventEditor<WorldPair, WorldPairEvent> { }
}
#endif
