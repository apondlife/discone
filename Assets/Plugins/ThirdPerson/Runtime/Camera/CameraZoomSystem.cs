using System;
using Soil;
using UnityEngine;

namespace ThirdPerson {

[Serializable]
sealed class CameraZoomSystem: SimpleSystem<CameraContainer> {
    // -- System --
    protected override Phase<CameraContainer> InitInitialPhase() {
        return Zooming;
    }

    public override void Init(CameraContainer c) {
        base.Init(c);

        c.State.Next.Fov = c.Tuning.Fov.Evaluate(0);
    }

    // -- Tracking --
    static readonly Phase<CameraContainer> Zooming = new("Zooming",
        update: (delta, _, c) => {
            var destFov = c.Tuning.Fov.Evaluate(
                Mathf.InverseLerp(
                    c.Tuning.FovTargetMinSpeed,
                    c.Tuning.FovTargetMaxSpeed,
                    c.State.Character.Next.Velocity.magnitude
                )
            );

            c.State.Next.Fov = Mathf.MoveTowards(
                c.State.Next.Fov,
                destFov,
                c.Tuning.FovSpeed * delta
            );
        }
    );
}

}