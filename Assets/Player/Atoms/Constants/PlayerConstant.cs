using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Constant of type `Player`. Inherits from `AtomBaseVariable&lt;Player&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-teal")]
    [CreateAssetMenu(menuName = "Unity Atoms/Constants/Player", fileName = "PlayerConstant")]
    public sealed class PlayerConstant : AtomBaseVariable<Player> { }
}
