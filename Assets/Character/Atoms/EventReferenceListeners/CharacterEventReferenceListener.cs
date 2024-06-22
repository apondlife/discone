using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event Reference Listener of type `Character`. Inherits from `AtomEventReferenceListener&lt;Character, CharacterEvent, CharacterEventReference, CharacterUnityEvent&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-orange")]
    [AddComponentMenu("Unity Atoms/Listeners/Character Event Reference Listener")]
    public sealed class CharacterEventReferenceListener : AtomEventReferenceListener<
        Character,
        CharacterEvent,
        CharacterEventReference,
        CharacterUnityEvent>
    { }
}
