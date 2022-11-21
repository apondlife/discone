using System;
using UnityEngine.Events;
using Discone;

namespace UnityAtoms.Discone
{
    /// <summary>
    /// None generic Unity Event of type `Region`. Inherits from `UnityEvent&lt;Region&gt;`.
    /// </summary>
    [Serializable]
    public sealed class RegionUnityEvent : UnityEvent<Region> { }
}
