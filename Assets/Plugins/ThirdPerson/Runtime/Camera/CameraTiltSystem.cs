using System;
using Soil;
using UnityEngine;

namespace ThirdPerson {

[Serializable]
sealed class CameraTiltSystem: CameraSystem {
    // -- System --
    protected override Phase InitInitialPhase() {
        return Tilting;
    }

    public override void Init() {
        base.Init();
    }

    // -- Tracking --
    Phase Tilting => new(
        name: "Tilting",
        update: Tilting_Update
    );

    void Tilting_Update(float delta) {
        // get angle between tilt up and camera up
        var tilt = Vector3.SignedAngle(
            m_State.Up,
            Vector3.ProjectOnPlane(m_State.Character.Tilt * Vector3.up, m_State.Forward),
            m_State.Forward
        );

        // map angle from [0, 360] to [-180, 180]
        if (tilt > 180.0f) {
            tilt -= 360.0f;
        }

        // TODO: smoothing with a finite end time (tween)
        m_State.Dutch = Mathf.LerpAngle(
            m_State.Dutch,
            tilt * m_Tuning.DutchScale,
            m_Tuning.DutchSmoothing
        );
    }
}

}