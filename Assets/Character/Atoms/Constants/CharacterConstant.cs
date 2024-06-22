using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Constant of type `Character`. Inherits from `AtomBaseVariable&lt;Character&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-teal")]
    [CreateAssetMenu(menuName = "Unity Atoms/Constants/Character", fileName = "CharacterConstant")]
    public sealed class CharacterConstant : AtomBaseVariable<Character> { }
}
