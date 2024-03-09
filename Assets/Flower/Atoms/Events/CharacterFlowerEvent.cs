using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `CharacterFlower`. Inherits from `AtomEvent&lt;CharacterFlower&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/CharacterFlower", fileName = "CharacterFlowerEvent")]
    public sealed class CharacterFlowerEvent : AtomEvent<CharacterFlower>
    {
    }
}