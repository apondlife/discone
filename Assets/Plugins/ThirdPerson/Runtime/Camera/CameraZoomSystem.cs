using System;
using Soil;
using UnityEngine;

namespace ThirdPerson {

using Container = CameraContainer;
using Phase = Phase<CameraContainer>;

[Serializable]
sealed class CameraZoomSystem: SimpleSystem<Container> {
    // -- System --
    protected override Phase InitInitialPhase() {
        return Zooming;
    }

    public override void Init(Container c) {
        base.Init(c);

        c.State.Fov = c.Tuning.Fov.Evaluate(0);
    }

    // -- Tracking --
    Phase Zooming => new(
        name: "Zooming",
        update: Zooming_Update
    );

    void Zooming_Update(float delta, Container c) {
        var destFov = c.Tuning.Fov.Evaluate(
            Mathf.InverseLerp(
                c.Tuning.FovTargetMinSpeed,
                c.Tuning.FovTargetMaxSpeed,
                c.State.Character.Next.Velocity.magnitude
            )
        );

        c.State.Next.Fov = Mathf.MoveTowards(
            c.State.Fov,
            destFov,
            c.Tuning.FovSpeed * delta
        );
    }
}

}