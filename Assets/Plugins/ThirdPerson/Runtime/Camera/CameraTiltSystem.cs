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
        update: Tilting_Update
    );

    static void Tilting_Update(float delta, System<CameraContainer> _, CameraContainer c) {
        var cameraRight = Vector3.Cross(c.State.Next.Forward, c.State.Next.Up);

        var acceleration = c.State.Character.Next.Acceleration;
        var accelerationCross = Vector3.Dot(acceleration, cameraRight);
        var accelerationAngle = Mathf.Sign(accelerationCross) * c.Tuning.Tilt_AccelerationAngle.Evaluate(Mathf.Abs(accelerationCross));

        // TODO: DynamicEase?
        c.State.Next.Tilt = Mathf.SmoothDampAngle(
            c.State.Curr.Tilt,
            accelerationAngle,
            ref c.State.Curr.TiltSpeed,
            c.Tuning.Tilt_Duration
        );
    }
}

}