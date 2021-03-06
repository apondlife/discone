using UnityEngine;
using System;

namespace UnityAtoms
{
    /// <summary>
    /// Variable of type `DisconePlayer`. Inherits from `AtomVariable&lt;DisconePlayer, DisconePlayerPair, DisconePlayerEvent, DisconePlayerPairEvent, DisconePlayerDisconePlayerFunction&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-lush")]
    [CreateAssetMenu(menuName = "Unity Atoms/Variables/DisconePlayer", fileName = "DisconePlayerVariable")]
    public sealed class DisconePlayerVariable : AtomVariable<DisconePlayer, DisconePlayerPair, DisconePlayerEvent, DisconePlayerPairEvent, DisconePlayerDisconePlayerFunction>
    {
        protected override bool ValueEquals(DisconePlayer other)
        {
            return other == Value;
        }
    }
}
