using UnityEngine;

namespace ThirdPerson {

/// how the character tilts on the ground
sealed class TiltSystem : CharacterSystem {
    // -- lifetime --
    public TiltSystem(CharacterData character)
        : base(character) {
    }

    protected override CharacterPhase InitInitialPhase() {
        return NotTilting;
    }

    // -- NotTilting --
    CharacterPhase NotTilting => new CharacterPhase(
        name: "NotTilting",
        update: NotTilting_Update
    );

    void NotTilting_Update() {
        var acceleration = Vector3.ProjectOnPlane(m_State.Acceleration, Vector3.up);
        if (acceleration.sqrMagnitude != 0.0f) {
            ChangeTo(Tilting);
            return;
        }

        InterpolateTilt(Quaternion.identity);
    }

    // -- Tilting --
    CharacterPhase Tilting => new CharacterPhase(
        name: "Tilting",
        update: Tilting_Update
    );

    void Tilting_Update() {
        var acceleration = Vector3.ProjectOnPlane(m_State.Acceleration, Vector3.up);
        if (acceleration.sqrMagnitude == 0.0f) {
            ChangeTo(NotTilting);
            return;
        }

        var tiltAngle = Mathf.Clamp(
            acceleration.magnitude / m_Tunables.Acceleration * m_Tunables.TiltForBaseAcceleration,
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
        m_State.Tilt = Quaternion.Slerp(
            m_State.Tilt,
            target,
            m_Tunables.TiltSmoothing
        );
    }
}

}