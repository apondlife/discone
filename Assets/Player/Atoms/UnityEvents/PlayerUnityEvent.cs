using System;
using UnityEngine.Events;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// None generic Unity Event of type `Player`. Inherits from `UnityEvent&lt;Player&gt;`.
    /// </summary>
    [Serializable]
    public sealed class PlayerUnityEvent : UnityEvent<Player> { }
}
