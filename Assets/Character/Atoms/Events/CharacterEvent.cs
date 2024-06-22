using Discone;
using UnityEngine;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `Character`. Inherits from `AtomEvent&lt;Character&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/Character", fileName = "CharacterEvent")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "UnityAtoms", "CharacterEvent")]
    public sealed class CharacterEvent : AtomEvent<Character>
    {
    }
}