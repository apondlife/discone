using UnityEngine;
using Discone;

namespace UnityAtoms.Discone
{
    /// <summary>
    /// Value List of type `Region`. Inherits from `AtomValueList&lt;Region, RegionEvent&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-piglet")]
    [CreateAssetMenu(menuName = "Unity Atoms/Value Lists/Region", fileName = "RegionValueList")]
    public sealed class RegionValueList : AtomValueList<Region, RegionEvent> { }
}
