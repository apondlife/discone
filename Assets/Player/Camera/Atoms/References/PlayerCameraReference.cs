using System;
using UnityAtoms.BaseAtoms;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Reference of type `PlayerCamera`. Inherits from `AtomReference&lt;PlayerCamera, PlayerCameraPair, PlayerCameraConstant, PlayerCameraVariable, PlayerCameraEvent, PlayerCameraPairEvent, PlayerCameraPlayerCameraFunction, PlayerCameraVariableInstancer, AtomCollection, AtomList&gt;`.
    /// </summary>
    [Serializable]
    public sealed class PlayerCameraReference : AtomReference<
        PlayerCamera,
        PlayerCameraPair,
        PlayerCameraConstant,
        PlayerCameraVariable,
        PlayerCameraEvent,
        PlayerCameraPairEvent,
        PlayerCameraPlayerCameraFunction,
        PlayerCameraVariableInstancer>, IEquatable<PlayerCameraReference>
    {
        public PlayerCameraReference() : base() { }
        public PlayerCameraReference(PlayerCamera value) : base(value) { }
        public bool Equals(PlayerCameraReference other) { return base.Equals(other); }
        protected override bool ValueEquals(PlayerCamera other)
        {
            throw new NotImplementedException();
        }
    }
}
