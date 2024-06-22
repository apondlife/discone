#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `PlayerPair`. Inherits from `AtomDrawer&lt;PlayerPairEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(PlayerPairEvent))]
    public class PlayerPairEventDrawer : AtomDrawer<PlayerPairEvent> { }
}
#endif