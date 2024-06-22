using UnityEditor;
using Discone;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Variable Inspector of type `Character`. Inherits from `AtomVariableEditor`
    /// </summary>
    [CustomEditor(typeof(CharacterVariable))]
    public sealed class CharacterVariableEditor : AtomVariableEditor<Character, CharacterPair> { }
}