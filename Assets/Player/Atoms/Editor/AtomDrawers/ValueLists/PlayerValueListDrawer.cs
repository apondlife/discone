#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Value List property drawer of type `Player`. Inherits from `AtomDrawer&lt;PlayerValueList&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(PlayerValueList))]
    public class PlayerValueListDrawer : AtomDrawer<PlayerValueList> { }
}
#endif
