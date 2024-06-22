using UnityEngine;
using UnityAtoms.BaseAtoms;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Set variable value Action of type `Player`. Inherits from `SetVariableValue&lt;Player, PlayerPair, PlayerVariable, PlayerConstant, PlayerReference, PlayerEvent, PlayerPairEvent, PlayerVariableInstancer&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-purple")]
    [CreateAssetMenu(menuName = "Unity Atoms/Actions/Set Variable Value/Player", fileName = "SetPlayerVariableValue")]
    public sealed class SetPlayerVariableValue : SetVariableValue<
        Player,
        PlayerPair,
        PlayerVariable,
        PlayerConstant,
        PlayerReference,
        PlayerEvent,
        PlayerPairEvent,
        PlayerPlayerFunction,
        PlayerVariableInstancer>
    { }
}
