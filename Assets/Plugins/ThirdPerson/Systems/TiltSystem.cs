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
        get => m_State.Next.TiltState;
    }

    // -- NotTilting --
    Phase NotTilting => new Phase(
        "NotTilting",
        update: NotTilting_Update
    );

    void NotTilting_Update(float _) {
        var acceleration = Vector3.ProjectOnPlane(m_State.Acceleration, Vector3.up);
        if (acceleration.sqrMagnitude != 0.0f) {
            ChangeTo(Tilting);
            return;
        }

        InterpolateTilt(Quaternion.identity);
    }

    // -- Tilting --
    Phase Tilting => new Phase(
        "Tilting",
        update: Tilting_Update
    );

    void Tilting_Update(float _) {
        var acceleration = Vector3.ProjectOnPlane(m_State.Acceleration, Vector3.up);
        if (acceleration.sqrMagnitude == 0.0f) {
            ChangeTo(NotTilting);
            return;
        }

        var tiltAngle = Mathf.Clamp(
            acceleration.magnitude / m_Tunables.Horizontal_Acceleration * m_Tunables.TiltForBaseAcceleration,
            0,
            m_Tunables.MaxTilt
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
        m_State.Next.Tilt = Quaternion.Slerp(
            m_State.Next.Tilt,
            target,
            m_Tunables.TiltSmoothing
        );
    }
}

}