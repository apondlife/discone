using UnityEditor;
using UnityAtoms.Editor;
using Discone;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Variable Inspector of type `Player`. Inherits from `AtomVariableEditor`
    /// </summary>
    [CustomEditor(typeof(PlayerVariable))]
    public sealed class PlayerVariableEditor : AtomVariableEditor<Player, PlayerPair> { }
}
