using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Value List of type `Character`. Inherits from `AtomValueList&lt;Character, CharacterEvent&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-piglet")]
    [CreateAssetMenu(menuName = "Unity Atoms/Value Lists/Character", fileName = "CharacterValueList")]
    public sealed class CharacterValueList : AtomValueList<Character, CharacterEvent> { }
}
