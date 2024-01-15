using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace ThirdPerson {
    /// system state extensions
    partial class CharacterState {
        partial class Frame {
            /// .
            [FormerlySerializedAs("WallState")] public SystemState SurfaceState;
        }
    }

    /// how the character interacts with surfaces
    [Serializable]
    sealed class SurfaceSystem : CharacterSystem {
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
                return;
            }

            // in the air, rotate perceived towards zero
            RotatePerceptionTowards(Vector3.zero, c.State.Curr.Position, delta);
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

            //
            // get collision surfaces
            //

            var currSurface = c.State.Curr.MainSurface;
            var currNormal = currSurface.Normal;
            var currSurfaceScale = c.Tuning.Surface_AngleScale.Evaluate(currSurface.Angle);
            var currBias = c.Tuning.Surface_UpwardsVelocityBias * currSurfaceScale * Vector3.up;
            var currVelocityDir = Vector3.ProjectOnPlane(c.State.Curr.Velocity + currBias, currNormal).normalized;

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
            var inertiaDecayTime = c.Tuning.Surface_InertiaDecayTime.Evaluate(currSurface.Angle);
            var inertiaDecayScale = 1f - Mathf.Pow(0.01f, delta / inertiaDecayTime);

            // clamp decay so it doesn't bounce
            var inertiaDecayMag = inertia * Math.Min(inertiaDecayScale, 1f);

            //
            // rotate transfer direction based input di
            //

            // get input in curr surface space
            var inputUp = Vector3.Dot(c.Input.Move, currFwd);
            var inputRight = Vector3.Dot(c.Input.Move, currUpTg);
            var inputTg = (inputUp * currUp + inputRight * currUpTg).normalized;

            // rotate tangent by input di
            var diAngle = Vector3.SignedAngle(surfaceTg, inputTg, currNormal);
            var diAngleMag = Mathf.Abs(diAngle);
            var diAngleSign = Mathf.Sign(diAngle);
            var diRot = c.Tuning.Surface_TransferDiAngle.Evaluate(diAngleMag) * diAngleSign * c.Input.MoveMagnitude;

            // scale transfer by di
            var diScale = c.Tuning.Surface_TransferDiScale.Evaluate(diAngleMag);

            //
            // scale based on angle between curr & perceived
            //
            var perceivedNormal = c.State.PerceivedSurface.Normal;
            var deltaAngle = Vector3.Angle(currNormal, perceivedNormal);
            var deltaScale = 1f + (c.Tuning.Surface_DeltaScale.Evaluate(deltaAngle) - 1f) * perceivedNormal.magnitude;

            //
            // add impulse along transfer tangent
            //
            var transferTg = Quaternion.AngleAxis(diRot, currNormal) * surfaceTg;
            var transferScale = currSurfaceScale * diScale * deltaScale;
            var transferImpulse = transferScale * inertiaDecayMag * transferTg;

            //
            // update physics frame
            //

            // add magnet/grip towards the wall so we don't let go
            var force = -c.Tuning.Surface_Grip * currSurfaceScale * currNormal;

            // TODO: add friction (is this friction?)
            // add upwards pull / surface gravity
            force += c.Tuning.Surface_UpwardsGrip * currSurfaceScale * currUp;

            // add transfer impulse
            var impulse = transferImpulse;

            // update frame
            c.State.Next.Force += force;
            c.State.Next.Velocity += impulse;
            c.State.Next.Inertia -= inertiaDecayMag;

            // rotate perceived towards the current surface
            RotatePerceptionTowards(currNormal, currSurface.Point, delta);

            // debug drawing
            DebugDraw.Push(
                "surface-normal",
                currSurface.Point,
                currNormal,
                new DebugDraw.Config(new Color(0.0f, 0.4f, 1.0f), tags: DebugDraw.Tag.Surface)
            );

            DebugDraw.Push(
                "surface-tangent",
                currSurface.Point,
                surfaceTg,
                new DebugDraw.Config(new Color(0.9f, 0.6f, 0.2f), tags: DebugDraw.Tag.Surface)
            );

            DebugDraw.Push(
                "surface-transfer",
                c.State.Curr.Position,
                transferImpulse,
                new DebugDraw.Config(new Color(1f, 1f, 0f), tags: DebugDraw.Tag.Surface)
            );

            DebugDraw.Push(
                "surface-inertia-pre",
                c.State.Curr.Position,
                c.State.Curr.Inertia * -currNormal,
                new DebugDraw.Config(new Color(0.1f, 0.8f, 0.5f), tags: DebugDraw.Tag.Surface, width: 2f)
            );

            DebugDraw.Push(
                "surface-inertia-post",
                c.State.Curr.Position,
                c.State.Next.Inertia * -currNormal,
                new DebugDraw.Config(new Color(0f, 1f, 0f), tags: DebugDraw.Tag.Surface)
            );
        }

        // -- commands --
        /// rotate perceived normal towards the destination vector
        void RotatePerceptionTowards(Vector3 dir, Vector3 pos, float delta) {
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