using UnityEngine;
using System;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Variable of type `PlayerCamera`. Inherits from `AtomVariable&lt;PlayerCamera, PlayerCameraPair, PlayerCameraEvent, PlayerCameraPairEvent, PlayerCameraPlayerCameraFunction&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-lush")]
    [CreateAssetMenu(menuName = "Unity Atoms/Variables/PlayerCamera", fileName = "PlayerCameraVariable")]
    public sealed class PlayerCameraVariable : AtomVariable<PlayerCamera, PlayerCameraPair, PlayerCameraEvent, PlayerCameraPairEvent, PlayerCameraPlayerCameraFunction>
    {
        protected override bool ValueEquals(PlayerCamera other) {
            return Value == other;
        }
    }
}