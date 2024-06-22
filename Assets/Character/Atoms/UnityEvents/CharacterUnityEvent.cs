using System;
using UnityEngine.Events;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// None generic Unity Event of type `Character`. Inherits from `UnityEvent&lt;Character&gt;`.
    /// </summary>
    [Serializable]
    public sealed class CharacterUnityEvent : UnityEvent<Character> { }
}
