using System;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.Serialization;

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
        var surface = c.State.Curr.MainSurface;
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
        if (!c.State.Curr.IsColliding) {
            ChangeTo(NotOnSurface);
            return;
        }

        // get current surface
        // AAA: investigate if we need to project velocity on plane
        var currSurface = c.State.Curr.MainSurface;
        var currNormal = currSurface.Normal;
        var currVelocityDir = c.State.Curr.Velocity.normalized;
        // var currVelocityDir = Vector3.ProjectOnPlane(c.State.Curr.Velocity, currNormal).normalized;

        // find "up" direction; if none, fallback to velocity & then forward
        var currUp = Vector3.ProjectOnPlane(Vector3.up, currNormal).normalized;
        if (currUp == Vector3.zero) {
            currUp = currVelocityDir;
        }

        if (currUp == Vector3.zero) {
            currUp = Vector3.ProjectOnPlane(c.State.Curr.Forward, currNormal).normalized;
        }

        var currUpTg = Vector3.Cross(currNormal, currUp);
        var currFwd = -Vector3.ProjectOnPlane(currSurface.Normal, Vector3.up).normalized;

        // find the previous surface
        var prevSurface = c.State.Prev.MainSurface;
        var prevNormal = prevSurface.Normal;

        // find the transfer tangent between the surfaces
        // AAA: store when surfaces change?
        var transferTg = Vector3.zero;

        // if previously on a surface, find surface rotation
        if (prevSurface.IsSome) {
            transferTg = Vector3.Cross(currNormal, Vector3.Cross(prevNormal, currNormal));
        }
        // if coming from air, use character velocity, if no velocity, use "up"
        else {
            transferTg = currVelocityDir != Vector3.zero ? currVelocityDir : currUp;
        }

        // AAA: scale transfer based on the angle between currSurface.Normal and lastSurface.Normal
        // var surfacePrev = c.State.Curr.PrevSurface;
        // var surfacePrevTg = surfacePrev.IsSome
        //     ? Vector3.ProjectOnPlane(surfacePrev.Normal, currSurface.Normal).normalized
        //     : currUp;

        // calculate added acceleration
        var acceleration = Vector3.zero;

        // get angle between surface and perceived surface
        var surfacePerceivedAngle = Mathf.Abs(90f - Vector3.Angle(
            currSurface.Normal,
            c.State.Curr.PerceivedSurface.Normal
        ));
        var surfacePerceivedScale = 1f - (surfacePerceivedAngle / 90f);

        // get input in curr surface space
        var inputUp = Vector3.Dot(c.Input.Move, currFwd);
        var inputRight = Vector3.Dot(c.Input.Move, currUpTg);
        var inputTg = (inputUp * currUp + inputRight * currUpTg).normalized;

        // AAA: find surface-based transfer scale
        // apply di rotation based on the input direction
        var diAngle = Vector3.SignedAngle(transferTg, inputTg, currNormal);
        var diAngleMag = Mathf.Abs(diAngle);
        var diAngleSign = Mathf.Sign(diAngle);
        var diRot = c.Tuning.Surface_TransferDiAngle.Evaluate(diAngleMag) * diAngleSign * c.Input.MoveMagnitude;

        transferTg = Quaternion.AngleAxis(diRot, currNormal) * transferTg;

        // transfer inertia up new surface w/ di
        var inertia = c.State.Curr.Inertia;

        // calculate the decay to hit 1% of the inertia over a fixed interval
        // TODO: can we optimize this pow by inverting this and showing the half-life as a debug query? -ty
        var inertiaDecayTime = c.Tuning.Surface_InertiaDecayTime.Evaluate(currSurface.Angle);
        var inertiaDecayScale = 1f - Mathf.Pow(0.01f, delta / inertiaDecayTime);
        inertiaDecayScale = 1f;

        // clamp decay so it doesn't bounce
        var inertiaDecayMag = Math.Min(inertia * inertiaDecayScale, inertia);

        // scale transfer based on surface & di
        var surfaceScale = c.Tuning.Surface_TransferScale.Evaluate(currSurface.Angle);
        var diScale = c.Tuning.Surface_TransferDiScale.Evaluate(diAngleMag);
        var transferAttack = c.Tuning.Surface_TransferAttack.Evaluate(surfacePerceivedScale);

        // AAA
        surfaceScale = 1f;
        transferAttack = 1f;

        // and transfer it along the surface tangent
        var transferMag = inertiaDecayMag * surfaceScale * diScale * transferAttack / delta;
        var transferImpulse = transferMag * transferTg;
        acceleration += transferImpulse;

        // add surface gravity
        // var surfaceGravity = c.Input.IsSurfaceHoldPressed ? c.Tuning.Surface_HoldGravity : c.Tuning.Surface_Gravity;;

        // scale by surface angle
        // var surfaceAngleScale = c.Tuning.Surface_AngleScale.Evaluate(surface.Angle);
        // var surfaceAcceleration = c.Tuning.Surface_Acceleration(surfaceGravity);
        // acceleration += surfaceAcceleration * surfaceAngleScale * surfaceUp;

        // add magnet/grip to push us towards the wall so we don't let go
        acceleration -= c.Tuning.Surface_Grip.Evaluate(currSurface.Angle) * currNormal;

        // update state
        c.State.Next.Inertia -= inertiaDecayMag;
        c.State.Next.Force += acceleration;

        DebugDraw.Push(
            "force-surface",
            c.State.Next.Position,
            acceleration,
            new DebugDraw.Config(new Color(1f, 1f, 0f))
        );
    }
}

}