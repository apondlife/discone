using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event Reference Listener of type `PlayerPair`. Inherits from `AtomEventReferenceListener&lt;PlayerPair, PlayerPairEvent, PlayerPairEventReference, PlayerPairUnityEvent&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-orange")]
    [AddComponentMenu("Unity Atoms/Listeners/PlayerPair Event Reference Listener")]
    public sealed class PlayerPairEventReferenceListener : AtomEventReferenceListener<
        PlayerPair,
        PlayerPairEvent,
        PlayerPairEventReference,
        PlayerPairUnityEvent>
    { }
}
