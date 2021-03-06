using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Variable Inspector of type `DisconePlayer`. Inherits from `AtomVariableEditor`
    /// </summary>
    [CustomEditor(typeof(DisconePlayerVariable))]
    public sealed class DisconePlayerVariableEditor : AtomVariableEditor<DisconePlayer, DisconePlayerPair> { }
}
