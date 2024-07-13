#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using Discone;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `Flower`. Inherits from `AtomEventEditor&lt;Flower, FlowerEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(FlowerEvent))]
    public sealed class FlowerEventEditor : AtomEventEditor<Flower, FlowerEvent> { }
}
#endif