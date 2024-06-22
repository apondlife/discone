using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event Instancer of type `Character`. Inherits from `AtomEventInstancer&lt;Character, CharacterEvent&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-sign-blue")]
    [AddComponentMenu("Unity Atoms/Event Instancers/Character Event Instancer")]
    public class CharacterEventInstancer : AtomEventInstancer<Character, CharacterEvent> { }
}
