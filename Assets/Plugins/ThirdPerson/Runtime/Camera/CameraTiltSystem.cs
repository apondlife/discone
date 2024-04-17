using System;
using Soil;
using UnityEngine;

namespace ThirdPerson {

using Container = CameraContainer;
using Phase = Phase<CameraContainer>;

[Serializable]
sealed class CameraTiltSystem: SimpleSystem<Container> {
    // -- System --
    protected override Phase InitInitialPhase() {
        return Tilting;
    }

    // -- Tracking --
    Phase Tilting => new(
        name: "Tilting",
        update: Tilting_Update
    );

    void Tilting_Update(float delta, Container c) {
        // get angle between tilt up and camera up
        var tilt = Vector3.SignedAngle(
            c.State.Up,
            Vector3.ProjectOnPlane(c.State.Character.Tilt * Vector3.up, c.State.Forward),
            c.State.Forward
        );

        // map angle from [0, 360] to [-180, 180]
        if (tilt > 180.0f) {
            tilt -= 360.0f;
        }

        // TODO: smoothing with a finite end time (tween)
        c.State.Dutch = Mathf.LerpAngle(
            c.State.Dutch,
            tilt * c.Tuning.DutchScale,
            c.Tuning.DutchSmoothing
        );
    }
}

}