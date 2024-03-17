#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;
using Discone;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `GameStep`. Inherits from `AtomEventEditor&lt;GameStep, GameStepEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(GameStepEvent))]
    public sealed class GameStepEventEditor : AtomEventEditor<GameStep, GameStepEvent> { }
}
#endif
