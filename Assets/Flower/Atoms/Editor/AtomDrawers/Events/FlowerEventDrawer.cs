#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `Flower`. Inherits from `AtomDrawer&lt;FlowerEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(FlowerEvent))]
    public class FlowerEventDrawer : AtomDrawer<FlowerEvent> { }
}
#endif