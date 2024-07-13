using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `Flower`. Inherits from `AtomEvent&lt;Flower&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/Flower", fileName = "FlowerEvent")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "UnityAtoms", "Assembly-CSharp", "CharacterFlowerEvent")]
    public sealed class FlowerEvent : AtomEvent<Flower>
    {
    }
}