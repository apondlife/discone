using System;
using Soil;
using UnityEngine;

namespace ThirdPerson {

[Serializable]
sealed class CameraTiltSystem: SimpleSystem<CameraContainer> {
    // -- System --
    protected override Phase<CameraContainer> InitInitialPhase() {
        return Tilting;
    }

    // -- Tracking --
    static readonly Phase<CameraContainer> Tilting = new("Tilting",
        update: (delta, _, c) => {
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
    );

}

}