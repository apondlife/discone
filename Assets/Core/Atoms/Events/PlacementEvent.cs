using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `Placement`. Inherits from `AtomEvent&lt;Placement&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/Placement", fileName = "PlacementEvent")]
    public sealed class PlacementEvent : AtomEvent<Placement>
    {
    }
}
