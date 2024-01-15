using UnityEngine;
using System;

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
sealed class TiltSystem : CharacterSystem {
    // -- System --
    protected override Phase InitInitialPhase() {
        return NotTilting;
    }

    protected override SystemState State {
        get => c.State.Next.TiltState;
        set => c.State.Next.TiltState = value;
    }

    // -- NotTilting --
    Phase NotTilting => new(
        "NotTilting",
        update: NotTilting_Update
    );

    void NotTilting_Update(float _) {
        var acceleration = Vector3.ProjectOnPlane(c.State.Curr.Acceleration, Vector3.up);
        if (acceleration.sqrMagnitude != 0.0f) {
            ChangeTo(Tilting);
            return;
        }

        InterpolateTilt(Quaternion.identity);
    }

    // -- Tilting --
    Phase Tilting => new(
        "Tilting",
        update: Tilting_Update
    );

    void Tilting_Update(float _) {
        var acceleration = Vector3.ProjectOnPlane(c.State.Curr.Acceleration, Vector3.up);
        if (acceleration.sqrMagnitude == 0.0f) {
            ChangeTo(NotTilting);
            return;
        }

        var tiltAngle = Mathf.Clamp(
            acceleration.magnitude / c.Tuning.Horizontal_Acceleration * c.Tuning.TiltForBaseAcceleration,
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

        InterpolateTilt(tilt);
    }

    // -- commands --
    /// smooth tilt towards target
    void InterpolateTilt(Quaternion target) {
        c.State.Next.Tilt = Quaternion.Slerp(
            c.State.Next.Tilt,
            target,
            c.Tuning.TiltSmoothing
        );
    }
}

}