using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Value List of type `Player`. Inherits from `AtomValueList&lt;Player, PlayerEvent&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-piglet")]
    [CreateAssetMenu(menuName = "Unity Atoms/Value Lists/Player", fileName = "PlayerValueList")]
    public sealed class PlayerValueList : AtomValueList<Player, PlayerEvent> { }
}
