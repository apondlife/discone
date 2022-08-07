using UnityEngine;

namespace UnityAtoms {
    /// <summary>
    /// Variable of type `World`. Inherits from `AtomVariable&lt;World, WorldPair, WorldEvent, WorldPairEvent, WorldWorldFunction&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-lush")]
    [CreateAssetMenu(menuName = "Unity Atoms/Variables/World", fileName = "WorldVariable")]
    public sealed class WorldVariable : AtomVariable<World, WorldPair, WorldEvent, WorldPairEvent, WorldWorldFunction> {
        protected override bool ValueEquals(World other) {
            return System.Object.ReferenceEquals(other, Value);
        }
    }
}
