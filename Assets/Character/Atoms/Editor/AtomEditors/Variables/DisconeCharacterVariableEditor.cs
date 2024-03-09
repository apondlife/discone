using UnityEditor;
using Discone;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Variable Inspector of type `DisconeCharacter`. Inherits from `AtomVariableEditor`
    /// </summary>
    [CustomEditor(typeof(DisconeCharacterVariable))]
    public sealed class DisconeCharacterVariableEditor : AtomVariableEditor<DisconeCharacter, DisconeCharacterPair> { }
}