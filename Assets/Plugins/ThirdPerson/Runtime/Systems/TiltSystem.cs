using UnityEngine;
using System;
using Soil;

// TODO: this system should be in the model
namespace ThirdPerson {

using Container = CharacterContainer;
using Phase = Phase<CharacterContainer>;

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
        get => m_Container.State.Next.TiltState;
        set => m_Container.State.Next.TiltState = value;
    }

    // -- NotTilting --
    Phase NotTilting => new(
        "NotTilting",
        update: NotTilting_Update
    );

    void NotTilting_Update(float _, Container c) {
        var acceleration = Vector3.ProjectOnPlane(c.State.Curr.Acceleration, Vector3.up);
        if (acceleration.sqrMagnitude != 0.0f) {
            ChangeTo(Tilting);
            return;
        }

        InterpolateTilt(Quaternion.identity, c);
    }

    // -- Tilting --
    Phase Tilting => new(
        "Tilting",
        update: Tilting_Update
    );

    void Tilting_Update(float _, Container c) {
        var acceleration = Vector3.ProjectOnPlane(c.State.Curr.Acceleration, Vector3.up);
        if (acceleration.sqrMagnitude == 0.0f) {
            ChangeTo(NotTilting);
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

    // -- commands --
    /// smooth tilt towards target
    void InterpolateTilt(Quaternion target, Container c) {
        c.State.Next.Tilt = Quaternion.Slerp(
            c.State.Next.Tilt,
            target,
            c.Tuning.TiltSmoothing
        );
    }
}

}