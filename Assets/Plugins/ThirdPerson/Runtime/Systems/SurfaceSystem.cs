using System;
using Soil;
using UnityEngine;
using Color = UnityEngine.Color;

namespace ThirdPerson {

/// system state extensions
partial class CharacterState {
    partial class Frame {
        /// .
        public SystemState SurfaceState;
    }
}

/// how the character interacts with surfaces
[Serializable]
sealed class SurfaceSystem: CharacterSystem {
    // -- System --
    protected override Phase<CharacterContainer> InitInitialPhase() {
        return NotOnSurface;
    }

    protected override SystemState State {
        get => c.State.Next.SurfaceState;
        set => c.State.Next.SurfaceState = value;
    }

    // -- NotOnSurface --
    static readonly Phase<CharacterContainer> NotOnSurface = new("NotOnSurface",
        update: NotOnSurface_Update
    );

    static void NotOnSurface_Update(float delta, System<CharacterContainer> s, CharacterContainer c) {
        // if we're on a surface, enter slide
        var surface = c.State.Curr.MainSurface;
        if (surface.IsSome) {
            s.ChangeToImmediate(OnSurface, delta);
            return;
        }

        // in the air, rotate perceived towards zero
        RotatePerceptionTowards(Vector3.zero, c.State.Curr.Position, delta, c);
    }

    // -- OnSurface --
    static readonly Phase<CharacterContainer> OnSurface = new("OnSurface",
        update: OnSurface_Update
    );

    static void OnSurface_Update(float delta, System<CharacterContainer> s, CharacterContainer c) {
        // if we left all surfaces, exit
        if (!c.State.Curr.IsColliding) {
            s.ChangeTo(NotOnSurface);
            return;
        }

        //
        // get perceived surface
        //
        var percSurface = c.State.Curr.PerceivedSurface;
        var percNormal = percSurface.Normal;
        var percScale = percNormal.magnitude;

        //
        // get collision surfaces
        //
        var currSurface = c.State.Curr.MainSurface;
        var currNormal = currSurface.Normal;
        var currAngle = currSurface.Angle;

        // HACK: scaling by perception here is a workaround for jumping into the ground adding transfer the character around
        var currVelocityBias = c.Tuning.Surface_UpwardsVelocityBias.Evaluate(currAngle) * Vector3.up;
        var currVelocityDir = Vector3.ProjectOnPlane(percScale * c.State.Curr.Velocity + currVelocityBias, currNormal).normalized;

        // find "up" direction; if none, fallback to velocity & then forward
        var currUp = Vector3.ProjectOnPlane(Vector3.up, currNormal).normalized;
        if (currUp == Vector3.zero) {
            currUp = currVelocityDir;
        }

        if (currUp == Vector3.zero) {
            // HACK: scaling by perception here is a workaround for jumping into the ground adding transfer the character around
            currUp = Vector3.ProjectOnPlane(percScale * c.State.Curr.Forward, currNormal).normalized;
        }

        var currUpTg = Vector3.Cross(currNormal, currUp);
        var currFwd = -Vector3.ProjectOnPlane(currNormal, Vector3.up).normalized;

        // find the previous surface
        var prevSurface = c.State.Prev.MainSurface;
        var prevNormal = prevSurface.Normal;

        //
        // find the next surface tangent
        //
        var surfaceTg = Vector3.zero;

        // if coming from air, use character velocity, if no velocity, use "up"
        if (prevSurface.IsNone) {
            surfaceTg = currVelocityDir != Vector3.zero ? currVelocityDir : currUp;
        }
        // if the surface changed, calculate a new transfer tangent
        else if (currNormal != prevNormal) {
            surfaceTg = Vector3.Cross(currNormal, Vector3.Cross(prevNormal, currNormal)).normalized;
        }
        // otherwise, use stored tangent
        else {
            surfaceTg = c.State.Curr.SurfaceTangent;
        }

        c.State.Next.SurfaceTangent = surfaceTg;

        //
        // get transfer from inertia decay into curr surface
        //
        var inertia = c.State.Curr.Inertia;

        // calculate the decay to hit 1% of the inertia over a fixed interval
        // TODO: can we optimize this pow by inverting this and showing the half-life as a debug query? -ty
        var inertiaDecayTime = c.Tuning.Surface_InertiaDecayTime.Evaluate(currAngle);
        var inertiaDecayScale = 1f - Mathf.Pow(0.01f, delta / inertiaDecayTime);

        // clamp decay so it doesn't bounce
        var inertiaDecayMag = inertia * Math.Min(inertiaDecayScale, 1f);

        //
        // rotate transfer direction based input di
        //

        // get input in curr surface space
        var inputUp = Vector3.Dot(c.Inputs.Move, currFwd);
        var inputRight = Vector3.Dot(c.Inputs.Move, currUpTg);
        var inputTg = (inputUp * currUp + inputRight * currUpTg).normalized;

        // rotate tangent by input di
        var diAngle = Vector3.SignedAngle(surfaceTg, inputTg, currNormal);
        var diAngleMag = Mathf.Abs(diAngle);
        var diAngleSign = Mathf.Sign(diAngle);
        var diRot = c.Tuning.Surface_DiRotation.Evaluate(diAngleMag) * diAngleSign * c.Inputs.MoveMagnitude;

        // scale transfer by di
        var diScale = c.Tuning.Surface_DiScale.Evaluate(diAngleMag);

        //
        // scale based on angle between curr & perceived
        //
        var deltaAngle = Vector3.Angle(currNormal, percNormal);
        var deltaScale = 1f + (c.Tuning.Surface_DeltaScale.Evaluate(deltaAngle) - 1f) * percScale;

        //
        // scale based surface angle
        //
        var angleScale = c.Tuning.Surface_AngleScale.Evaluate(currAngle);

        //
        // scale when landing (from air)
        //
        var landingScale = 1f;
        if (percSurface.IsNone) {
            landingScale = c.Tuning.Surface_LandingScale.Evaluate(currAngle);
        }

        //
        // add impulse along transfer tangent
        //
        var transferTg = Quaternion.AngleAxis(diRot, currNormal) * surfaceTg;
        var transferScale = angleScale * diScale * deltaScale * landingScale;
        var transferImpulse = transferScale * inertiaDecayMag * transferTg;

        //
        // update physics frame
        //

        // add magnet/grip towards the wall so that we don't let go
        var force = -c.Tuning.Surface_Grip.Evaluate(currAngle) * currNormal;

        // TODO: add friction (is this friction?)
        // add upwards pull / surface gravity
        var gripCurve = c.Tuning.Surface_VerticalGrip_Scale;
        var gripRange = c.State.Curr.Velocity.y switch {
            < 0 => c.Tuning.Surface_VerticalGrip_Down,
            _   => c.Tuning.Surface_VerticalGrip_Up
        };

        var grip = MapCurve.Evaluate(gripCurve, gripRange, currAngle);

        // project grip into surface
        force += grip * Vector3.ProjectOnPlane(Vector3.up, currNormal);

        // add transfer impulse
        var impulse = transferImpulse;

        // update frame
        c.State.Next.Force += force;
        c.State.Next.Velocity += impulse;
        c.State.Next.Inertia -= inertiaDecayMag;

        // rotate perceived towards the current surface
        RotatePerceptionTowards(currNormal, currSurface.Point, delta, c);

        // debug drawing
        DebugDraw.Push("surface-normal", currSurface.Point, currNormal, new DebugDraw.Config(new Color(0.0f, 0.4f, 1.0f), tags: DebugDraw.Tag.Surface));
        DebugDraw.Push("surface-tangent", currSurface.Point, surfaceTg, new DebugDraw.Config(new Color(0.9f, 0.6f, 0.2f), tags: DebugDraw.Tag.Surface));
        DebugDraw.Push("surface-transfer", c.State.Curr.Position, transferImpulse, new DebugDraw.Config(new Color(1f, 1f, 0f), tags: DebugDraw.Tag.Surface));
        DebugDraw.Push("surface-inertia-pre", c.State.Curr.Position, c.State.Curr.Inertia * -currNormal, new DebugDraw.Config(new Color(0.1f, 0.8f, 0.5f), tags: DebugDraw.Tag.Surface, width: 2f));
        DebugDraw.Push("surface-inertia-post", c.State.Curr.Position, c.State.Next.Inertia * -currNormal, new DebugDraw.Config(new Color(0f, 1f, 0f), tags: DebugDraw.Tag.Surface));
    }

    // -- commands --
    /// rotate perceived normal towards the destination vector
    static void RotatePerceptionTowards(
        Vector3 dir,
        Vector3 pos,
        float delta,
        CharacterContainer c
    ) {
        var nextSurface = c.State.Curr.PerceivedSurface;

        // move the perceived surface towards the current surface
        var normal = nextSurface.Normal;

        // rotate towards current surface
        var rotationSpeed = 0f;
        var normalMag = normal.sqrMagnitude;
        if (normalMag != 0f) {
            rotationSpeed = c.Tuning.Surface_PerceptionAngularSpeed * Mathf.Deg2Rad / normalMag;
        }

        // rotate perception towards current surface
        // TODO: maybe update the time since last touching the curr surface
        normal = Vector3.RotateTowards(
            normal,
            dir,
            rotationSpeed * delta,
            delta / c.Tuning.Surface_PerceptionDuration
        );

        DebugDraw.Push(
            "surface-perception",
            c.State.Curr.MainSurface.Point,
            normal,
            new DebugDraw.Config(new Color(0.3f, 0.8f, 1f), tags: DebugDraw.Tag.Surface)
        );

        // update next surface
        nextSurface.Point = pos;
        nextSurface.SetNormal(normal);
        c.State.Next.PerceivedSurface = nextSurface;
    }
}

}