using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Variable Inspector of type `Entities`. Inherits from `AtomVariableEditor`
    /// </summary>
    [CustomEditor(typeof(EntitiesVariable))]
    public sealed class EntitiesVariableEditor : AtomVariableEditor<Entities, EntitiesPair> { }
}
