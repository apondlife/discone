using System;
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

        // update to new surface collision
        // ???: could this work w/ a loop over every surface? how would we know prev surface per-iteration?
        var currSurface = c.State.Curr.MainSurface;
        var currNormal = currSurface.Normal;
        var currVelocityDir = Vector3.ProjectOnPlane(c.State.Curr.Velocity + c.State.Curr.Inertia, currNormal).normalized;

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
        // var transferDiAngle = Vector3.SignedAngle(surfacePrevTg, inputTg, currNormal);
        var transferDiAngle = 0f;
        var transferDiAngleMag = Mathf.Abs(transferDiAngle);
        var transferDiAngleSign = Mathf.Sign(transferDiAngle);

        var transferDiRot = c.Tuning.Surface_TransferDiAngle.Evaluate(transferDiAngleMag) * transferDiAngleSign * c.Input.MoveMagnitude;
        // transferTg = Quaternion.AngleAxis(transferDiRot, currNormal) * transferTg;

        // transfer inertia up new surface w/ di
        // TODO: should we consume tangent inertia as well? there's an issue when you hit wall & ground where
        // inertia is tangent due to our collision ordering prioritizing the most recent surface (fix collision ordering)
        var inertia = c.State.Curr.Inertia;
        // var inertiaTg = Vector3.ProjectOnPlane(inertia, surfaceNormal);
        // var inertiaNormal = inertia - inertiaTg;
        var inertiaNormal = Vector3.Project(inertia, currNormal);

        // calculate the decay to hit 1% of the inertia over a fixed interval
        // TODO: can we optimize this pow by inverting this and showing the half-life as a debug query? -ty
        var inertiaDecayTime = c.Tuning.Surface_InertiaDecayTime.Evaluate(currSurface.Angle);
        var inertiaDecayScale = 1f - Mathf.Pow(0.01f, delta / inertiaDecayTime);
        inertiaDecayScale = 1f;
        var inertiaDecay = inertiaNormal * inertiaDecayScale;

        // clamp decay so it doesn't bounce
        var inertiaDecayMag = Math.Min(inertiaDecay.magnitude, inertiaNormal.magnitude);
        inertiaDecay = Vector3.ClampMagnitude(inertiaDecay, inertiaDecayMag);

        // tune transfer
        var transferScale = c.Tuning.Surface_TransferScale.Evaluate(currSurface.Angle) / delta;
        var transferDiScale = c.Tuning.Surface_TransferDiScale.Evaluate(transferDiAngleMag);
        var transferAttack = c.Tuning.Surface_TransferAttack.Evaluate(surfacePerceivedScale);

        // AAA
        transferDiScale = 1f;
        transferAttack = 1f;
        transferScale = 1;

        // and transfer it along the surface tangent
        var transferMag = inertiaDecayMag * transferScale * transferDiScale * transferAttack;
        var transferImpulse = transferMag * transferTg;
        acceleration += transferImpulse / delta;
        DebugDraw.Push(
            $"transf-tg{0}",
            c.State.Next.Position,
            transferTg,
            new DebugDraw.Config(new Color(1f, 0.8f, 0f))
        );

        // add surface gravity
        // var surfaceGravity = c.Input.IsSurfaceHoldPressed ? c.Tuning.Surface_HoldGravity : c.Tuning.Surface_Gravity;;

        // scale by surface angle
        // var surfaceAngleScale = c.Tuning.Surface_AngleScale.Evaluate(surface.Angle);
        // var surfaceAcceleration = c.Tuning.Surface_Acceleration(surfaceGravity);
        // acceleration += surfaceAcceleration * surfaceAngleScale * surfaceUp;

        // update state
        c.State.Next.Inertia -= inertiaDecay;
        c.State.Next.Force += acceleration;

        DebugDraw.Push(
            "inertia-post",
            c.State.Next.Position,
            c.State.Next.Inertia
        );

        DebugDraw.Push(
            "acceleration-surf",
            c.State.Next.Position,
            acceleration,
            new DebugDraw.Config(new Color(1f, 1f, 0f))
        );
    }
}

}