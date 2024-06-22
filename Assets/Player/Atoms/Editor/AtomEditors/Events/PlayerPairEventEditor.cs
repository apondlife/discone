#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;
using Discone;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `PlayerPair`. Inherits from `AtomEventEditor&lt;PlayerPair, PlayerPairEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(PlayerPairEvent))]
    public sealed class PlayerPairEventEditor : AtomEventEditor<PlayerPair, PlayerPairEvent> { }
}
#endif
