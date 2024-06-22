using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event Instancer of type `PlayerPair`. Inherits from `AtomEventInstancer&lt;PlayerPair, PlayerPairEvent&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-sign-blue")]
    [AddComponentMenu("Unity Atoms/Event Instancers/PlayerPair Event Instancer")]
    public class PlayerPairEventInstancer : AtomEventInstancer<PlayerPair, PlayerPairEvent> { }
}
