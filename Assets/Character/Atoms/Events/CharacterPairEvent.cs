using UnityEngine;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `CharacterPair`. Inherits from `AtomEvent&lt;CharacterPair&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/CharacterPair", fileName = "CharacterPairEvent")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "UnityAtoms", "DisconeCharacterPairEvent")]
    public sealed class CharacterPairEvent : AtomEvent<CharacterPair>
    {
    }
}