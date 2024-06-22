using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event Instancer of type `CharacterPair`. Inherits from `AtomEventInstancer&lt;CharacterPair, CharacterPairEvent&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-sign-blue")]
    [AddComponentMenu("Unity Atoms/Event Instancers/CharacterPair Event Instancer")]
    public class CharacterPairEventInstancer : AtomEventInstancer<CharacterPair, CharacterPairEvent> { }
}
