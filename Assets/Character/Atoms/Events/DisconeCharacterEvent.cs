using Discone;
using UnityEngine;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `DisconeCharacter`. Inherits from `AtomEvent&lt;DisconeCharacter&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/DisconeCharacter", fileName = "DisconeCharacterEvent")]
    public sealed class DisconeCharacterEvent : AtomEvent<Character>
    {
    }
}