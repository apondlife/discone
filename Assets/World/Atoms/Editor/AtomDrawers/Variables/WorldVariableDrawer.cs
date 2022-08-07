#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Variable property drawer of type `World`. Inherits from `AtomDrawer&lt;WorldVariable&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(WorldVariable))]
    public class WorldVariableDrawer : VariableDrawer<WorldVariable> { }
}
#endif
