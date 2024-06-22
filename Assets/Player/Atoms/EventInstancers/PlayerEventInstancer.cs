using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event Instancer of type `Player`. Inherits from `AtomEventInstancer&lt;Player, PlayerEvent&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-sign-blue")]
    [AddComponentMenu("Unity Atoms/Event Instancers/Player Event Instancer")]
    public class PlayerEventInstancer : AtomEventInstancer<Player, PlayerEvent> { }
}
