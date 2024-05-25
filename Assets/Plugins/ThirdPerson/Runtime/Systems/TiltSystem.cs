using UnityEngine;
using System;
using Soil;

// TODO: this system should be in the model
namespace ThirdPerson {

/// system state extensions
partial class CharacterState {
    partial class Frame {
        /// .
        public SystemState TiltState;
    }
}

/// how the character tilts on the ground
[Serializable]
sealed class TiltSystem: CharacterSystem {
    // -- System --
    protected override Phase<CharacterContainer> InitInitialPhase() {
        return NotTilting;
    }

    protected override SystemState State {
        get => c.State.Next.TiltState;
        set => c.State.Next.TiltState = value;
    }

    // -- NotTilting --
    static readonly Phase<CharacterContainer> NotTilting = new("NotTilting",
        update: (_, s, c) => {
            var acceleration = Vector3.ProjectOnPlane(c.State.Curr.Acceleration, Vector3.up);
            if (acceleration.sqrMagnitude != 0.0f) {
                s.ChangeTo(Tilting);
                return;
            }

            InterpolateTilt(Quaternion.identity, c);
        }
    );

    // -- Tilting --
    static readonly Phase<CharacterContainer> Tilting = new("Tilting",
        update: (_, s, c) => {
            var acceleration = Vector3.ProjectOnPlane(c.State.Curr.Acceleration, Vector3.up);
            if (acceleration.sqrMagnitude == 0.0f) {
                s.ChangeTo(NotTilting);
                return;
            }

            var tiltAngle = Mathf.Clamp(
                acceleration.magnitude / c.Tuning.Surface_Acceleration.Evaluate(c.State.Curr.MainSurface.Angle) * c.Tuning.TiltForBaseAcceleration,
                0,
                c.Tuning.MaxTilt
            );

            var tiltAxis = Vector3.Cross(
                Vector3.up,
                acceleration.normalized
            );

            var tilt = Quaternion.AngleAxis(
                tiltAngle,
                tiltAxis.normalized
            );

            InterpolateTilt(tilt, c);
        }
    );

    // -- commands --
    /// smooth tilt towards target
    static void InterpolateTilt(Quaternion target, CharacterContainer c) {
        c.State.Next.Tilt = Quaternion.Slerp(
            c.State.Next.Tilt,
            target,
            c.Tuning.TiltSmoothing
        );
    }
}

}