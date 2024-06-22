using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event Reference Listener of type `CharacterPair`. Inherits from `AtomEventReferenceListener&lt;CharacterPair, CharacterPairEvent, CharacterPairEventReference, CharacterPairUnityEvent&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-orange")]
    [AddComponentMenu("Unity Atoms/Listeners/CharacterPair Event Reference Listener")]
    public sealed class CharacterPairEventReferenceListener : AtomEventReferenceListener<
        CharacterPair,
        CharacterPairEvent,
        CharacterPairEventReference,
        CharacterPairUnityEvent>
    { }
}
