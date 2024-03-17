#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `GameStep`. Inherits from `AtomDrawer&lt;GameStepEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(GameStepEvent))]
    public class GameStepEventDrawer : AtomDrawer<GameStepEvent> { }
}
#endif
