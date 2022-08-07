#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `WorldPair`. Inherits from `AtomDrawer&lt;WorldPairEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(WorldPairEvent))]
    public class WorldPairEventDrawer : AtomDrawer<WorldPairEvent> { }
}
#endif
