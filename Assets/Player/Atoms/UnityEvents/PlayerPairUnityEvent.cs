using System;
using UnityEngine.Events;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// None generic Unity Event of type `PlayerPair`. Inherits from `UnityEvent&lt;PlayerPair&gt;`.
    /// </summary>
    [Serializable]
    public sealed class PlayerPairUnityEvent : UnityEvent<PlayerPair> { }
}
