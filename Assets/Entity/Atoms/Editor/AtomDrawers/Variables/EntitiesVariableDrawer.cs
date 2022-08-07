#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Variable property drawer of type `Entities`. Inherits from `AtomDrawer&lt;EntitiesVariable&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(EntitiesVariable))]
    public class EntitiesVariableDrawer : VariableDrawer<EntitiesVariable> { }
}
#endif
