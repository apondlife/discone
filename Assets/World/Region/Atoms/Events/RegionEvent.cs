using UnityEngine;

namespace UnityAtoms.Discone
{
    /// <summary>
    /// Event of type `Region`. Inherits from `AtomEvent&lt;Region&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/Region", fileName = "RegionEvent")]
    public sealed class RegionEvent : AtomEvent<Region>
    {
    }
}
