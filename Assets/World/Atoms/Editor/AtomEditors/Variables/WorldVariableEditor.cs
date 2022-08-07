using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Variable Inspector of type `World`. Inherits from `AtomVariableEditor`
    /// </summary>
    [CustomEditor(typeof(WorldVariable))]
    public sealed class WorldVariableEditor : AtomVariableEditor<World, WorldPair> { }
}
