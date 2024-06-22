using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event Reference Listener of type `Player`. Inherits from `AtomEventReferenceListener&lt;Player, PlayerEvent, PlayerEventReference, PlayerUnityEvent&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-orange")]
    [AddComponentMenu("Unity Atoms/Listeners/Player Event Reference Listener")]
    public sealed class PlayerEventReferenceListener : AtomEventReferenceListener<
        Player,
        PlayerEvent,
        PlayerEventReference,
        PlayerUnityEvent>
    { }
}
