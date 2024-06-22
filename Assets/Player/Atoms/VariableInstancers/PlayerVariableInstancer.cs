using UnityEngine;
using UnityAtoms.BaseAtoms;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Variable Instancer of type `Player`. Inherits from `AtomVariableInstancer&lt;PlayerVariable, PlayerPair, Player, PlayerEvent, PlayerPairEvent, PlayerPlayerFunction&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-hotpink")]
    [AddComponentMenu("Unity Atoms/Variable Instancers/Player Variable Instancer")]
    public class PlayerVariableInstancer : AtomVariableInstancer<
        PlayerVariable,
        PlayerPair,
        Player,
        PlayerEvent,
        PlayerPairEvent,
        PlayerPlayerFunction>
    { }
}
