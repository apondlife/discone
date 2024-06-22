using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Variable of type `Player`. Inherits from `AtomVariable&lt;Player, PlayerPair, PlayerEvent, PlayerPairEvent, PlayerFunction&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-lush")]
    [CreateAssetMenu(menuName = "Unity Atoms/Variables/Player", fileName = "PlayerVariable")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "UnityAtoms", "DisconePlayerVariable")]
    public sealed class PlayerVariable : AtomVariable<Player, PlayerPair, PlayerEvent, PlayerPairEvent, PlayerPlayerFunction>
    {
        protected override bool ValueEquals(Player other)
        {
            return other == Value;
        }
    }
}