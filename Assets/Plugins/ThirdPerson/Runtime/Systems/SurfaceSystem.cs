using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace ThirdPerson {

/// system state extensions
partial class CharacterState {
    partial class Frame {
        /// .
        [FormerlySerializedAs("WallState")]
        public SystemState SurfaceState;
    }
}

/// how the character interacts with surfaces
[Serializable]
sealed class SurfaceSystem: CharacterSystem {
    // -- System --
    protected override Phase InitInitialPhase() {
        return NotOnSurface;
    }

    protected override SystemState State {
        get => c.State.Next.SurfaceState;
        set => c.State.Next.SurfaceState = value;
    }

    // -- NotOnSurface --
    Phase NotOnSurface => new(
        "NotOnSurface",
        update: NotOnSurface_Update
    );

    void NotOnSurface_Update(float delta) {
        // if we're on a surface, enter slide
        var surface = c.State.Curr.WallSurface;
        if (surface.IsSome) {
            ChangeToImmediate(SurfaceSlide, delta);
        }
    }

    // -- SurfaceSlide --
    Phase SurfaceSlide => new Phase(
        name: "SurfaceSlide",
        update: SurfaceSlide_Update
    );

    void SurfaceSlide_Update(float delta) {
        // if we left all surfaces, exit
        if (c.State.SurfaceCount == 0) {
            ChangeTo(NotOnSurface);
            return;
        }

        DebugDraw.Push("inertia-pre", c.State.Next.Position, c.State.Next.Inertia);
        Vector3 addedAcceleration = Vector3.zero;

        var prevNormal = Vector3.zero;
        for (var i = 0; i < c.State.Prev.SurfaceCount; i++) {
            prevNormal += c.State.Prev.Surfaces[i].Normal;
        }

        prevNormal = Vector3.Normalize(prevNormal);

        // update to new surface collision
        for (var i = 0; i < c.State.Next.SurfaceCount; i++) {
            var surface = c.State.Next.Surfaces[i];

            var surfaceNormal = surface.Normal;
            var surfaceUp = Vector3.ProjectOnPlane(Vector3.up, surface.Normal).normalized;
            var surfaceUpTg = Vector3.Cross(surfaceNormal, surfaceUp);
            var surfaceFwd = -Vector3.ProjectOnPlane(surface.Normal, Vector3.up).normalized;

            // var moveDir = 0.99999f * c.State.Next.Velocity + 0.00001f * Vector3.up;
            // var surfacePrevTg = Vector3.ProjectOnPlane(moveDir, surface.Normal).normalized;

            var surfacePrev = c.State.Next.PrevSurface;
            var surfacePrevTg = surfacePrev.IsSome
                ? Vector3.ProjectOnPlane(surfacePrev.Normal, surface.Normal).normalized
                : surfaceUp;

            // // AAA: this is bad cause velocity keeps changing and thus projection keeps changing
            // surfacePrevTg = surfaceUp;

            // calculate added acceleration
            var acceleration = Vector3.zero;

            // get angle between surface and perceived surface
            var surfacePerceivedAngle = Mathf.Abs(90f - Vector3.Angle(
                surface.Normal,
                c.State.Curr.PerceivedSurface.Normal
            ));
            var surfacePerceivedScale = 1f - (surfacePerceivedAngle / 90f);

            // get input in surface space
            var surfaceInputUp = Vector3.Dot(c.Input.Move, surfaceFwd);
            var surfaceInputRight = Vector3.Dot(c.Input.Move, surfaceUpTg);
            var surfaceInputTg = (surfaceInputUp * surfaceUp + surfaceInputRight * surfaceUpTg).normalized;

            // find surface-based transfer scale
            var transferDiAngle = Vector3.SignedAngle(surfacePrevTg, surfaceInputTg, surfaceNormal);
            var transferDiAngleMag = Mathf.Abs(transferDiAngle);
            var transferDiAngleSign = Mathf.Sign(transferDiAngle);

            var transferDiRot = c.Tuning.Surface_TransferDiAngle.Evaluate(transferDiAngleMag) * transferDiAngleSign * c.Input.MoveMagnitude;
            var transferTg = Quaternion.AngleAxis(transferDiRot, surfaceNormal) * surfacePrevTg;

            // transfer inertia up new surface w/ di
            // TODO: should we consume tangent inertia as well? there's an issue when you hit wall & ground where
            // inertia is tangent due to our collision ordering prioritizing the most recent surface (fix collision ordering)
            var inertia = c.State.Curr.Inertia;
            var inertiaTg = Vector3.ProjectOnPlane(inertia, surfaceNormal);
            var inertiaNormal = inertia - inertiaTg;

            // calculate the decay to hit 1% of the inertia over a fixed interval
            // TODO: can we optimize this pow by inverting this and showing the half-life as a debug query? -ty
            var inertiaDecayTime = c.Tuning.Surface_InertiaDecayTime.Evaluate(surface.Angle);
            var inertiaDecayScale = 1f - Mathf.Pow(0.01f, delta / inertiaDecayTime);
            var inertiaDecay = inertiaNormal * inertiaDecayScale;

            // clamp decay so it doesn't bounce
            var inertiaDecayMag = Math.Min(inertiaDecay.magnitude, inertiaNormal.magnitude);
            inertiaDecay = Vector3.ClampMagnitude(inertiaDecay, inertiaDecayMag);

            // tune transfer
            var transferDiScale = 1f;
            var transferAttack = 1f;
            var transferScale = c.Tuning.Surface_TransferScale.Evaluate(surface.Angle);
            // var transferDiScale = c.Tuning.Surface_TransferDiScale.Evaluate(transferDiAngleMag);
            // var transferAttack = c.Tuning.Surface_TransferAttack.Evaluate(surfacePerceivedScale);

            // and transfer it along the surface tangent
            var transferMag = inertiaDecayMag * transferScale * transferDiScale * transferAttack;
            var transferAcceleration = transferMag * transferTg;
            acceleration += transferAcceleration;
            DebugDraw.Push($"transf-tg{i}", c.State.Next.Position, transferTg);

            // add surface gravity
            // var surfaceGravity = c.Input.IsSurfaceHoldPressed ? c.Tuning.Surface_HoldGravity : c.Tuning.Surface_Gravity;;

            // scale by surface angle
            // var surfaceAngleScale = c.Tuning.Surface_AngleScale.Evaluate(surface.Angle);
            // var surfaceAcceleration = c.Tuning.Surface_Acceleration(surfaceGravity);
            // acceleration += surfaceAcceleration * surfaceAngleScale * surfaceUp;

            // update state
            c.State.Next.Inertia -= inertiaDecay;
            c.State.Next.Force += acceleration;
            addedAcceleration += acceleration;
        }

        DebugDraw.Push("inertia-post", c.State.Next.Position, c.State.Next.Inertia);
        Vector3 r = Random.insideUnitCircle.normalized;
        r.z = r.y;
        r = Vector3.zero;
        r.y = 0;
        DebugDraw.Push(
            "acceleration-surf",
            c.State.Next.Position + r,
            addedAcceleration
        );
    }
}

}