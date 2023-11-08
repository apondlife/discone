using System;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace ThirdPerson {

/// system state extensions
partial class CharacterState {
    partial class Frame {
        /// .
        public SystemState WallState;
    }
}

// TODO: make this surface system -ty
/// how the character interacts with surfaces
[Serializable]
sealed class WallSystem: CharacterSystem {
    // -- System --
    protected override Phase InitInitialPhase() {
        return NotOnWall;
    }

    protected override SystemState State {
        get => c.State.Next.WallState;
        set => c.State.Next.WallState = value;
    }

    // -- NotOnWall --
    Phase NotOnWall => new(
        "NotOnWall",
        update: NotOnWall_Update
    );

    void NotOnWall_Update(float delta) {
        // if we're on a wall, enter slide
        var wall = c.State.Curr.WallSurface;
        if (wall.IsSome) {
            ChangeToImmediate(WallSlide, delta);
        }
        // var wallAngleScale = c.Tuning.WallAngleScale.Evaluate(wall.Angle);
        // if (wallAngleScale > Mathf.Epsilon) {
        //     ChangeToImmediate(WallSlide, delta);
        // }
    }

    // -- WallSlide --
    Phase WallSlide => new Phase(
        name: "WallSlide",
        update: WallSlide_Update
    );

    void WallSlide_Update(float delta) {
        // if we left the wall, exit
        var wall = c.State.Curr.WallSurface;
        if (wall.IsNone) {
            ChangeTo(NotOnWall);
            return;
        }

        // update to new wall collision
        var wallNormal = wall.Normal;
        var wallUp = Vector3.ProjectOnPlane(Vector3.up, wall.Normal).normalized;
        var wallTg = Vector3.Cross(wallNormal, wallUp);
        var wallFwd = -Vector3.ProjectOnPlane(wall.Normal, Vector3.up).normalized;

        var wallSurfaceTg = c.State.Prev.WallSurface.IsSome
            ? Vector3.ProjectOnPlane(c.State.Prev.WallSurface.Normal, wall.Normal).normalized
            : wallUp;

        // calculate added acceleration
        var acceleration = Vector3.zero;

        // get delta between wall and perceived surface
        var wallToSurface = Vector3.Angle(
            c.State.Curr.WallSurface.Normal,
            c.State.Curr.PerceivedSurface.Normal
        );

        var normalAngleDelta = Mathf.Abs(90f - wallToSurface);
        var normalAngleScale = 1f - (normalAngleDelta / 90f);

        // get input in wall space
        var wallInputUp = Vector3.Dot(c.Input.Move, wallFwd);
        var wallInputRight = Vector3.Dot(c.Input.Move, wallTg);
        var wallInputTg = (wallInputUp * wallUp + wallInputRight * wallTg).normalized;

        // add a magnet to pull the character towards the surface
        // TODO: should this be something the character controller does?
        acceleration -= (c.Controller.ContactOffset / (delta * delta)) * wallNormal;

        // find surface-based transfer scale
        var transferDiAngle = Vector3.SignedAngle(wallSurfaceTg, wallInputTg, wallNormal);
        var transferDiAngleMag = Mathf.Abs(transferDiAngle);
        var transferDiAngleSign = Mathf.Sign(transferDiAngle);

        var transferDiRot = c.Tuning.WallTransferDiAngle.Evaluate(transferDiAngleMag) * transferDiAngleSign * c.Input.MoveMagnitude;
        var transferTg = Quaternion.AngleAxis(transferDiRot, wallNormal) * wallSurfaceTg;

        // transfer inertia up new surface w/ di
        // TODO: should we consume tangent inertia as well? there's an issue when you hit wall & ground where
        // inertia is tangent due to our collision ordering prioritizing the most recent surface (fix collision ordering)
        var inertia = c.State.Curr.Inertia;
        var inertiaTg = Vector3.ProjectOnPlane(inertia, wallNormal);
        var inertiaNormal = inertia - inertiaTg;

        // calculate the decay to hit 1% of the inertia over a fixed interval
        // TODO: can we optimize this pow by inverting this and showing the half-life as a debug query? -ty
        var inertiaDecayTime = c.Tuning.Surface_InertiaDecayTime.Evaluate(wall.Angle);
        var inertiaDecayScale = 1f - Mathf.Pow(0.01f, delta / inertiaDecayTime);
        var inertiaDecay = inertiaNormal * inertiaDecayScale;

        // clamp decay so it doesn't bounce
        var inertiaDecayMag = Math.Min(inertiaDecay.magnitude, inertiaNormal.magnitude);
        inertiaDecay = Vector3.ClampMagnitude(inertiaDecay, inertiaDecayMag);

        // tune transfer
        var transferScale = c.Tuning.Surface_TransferScale.Evaluate(wall.Angle);
        var transferDiScale = c.Tuning.WallTransferDiScale.Evaluate(transferDiAngleMag);
        var transferAttack = c.Tuning.Surface_TransferAttack.Evaluate(normalAngleScale);

        // and transfer it along the surface tangent
        var transferMag = inertiaDecayMag * transferScale * transferDiScale * transferAttack;
        var transferAcceleration = transferMag * transferTg;
        acceleration += transferAcceleration;

        // get angle (upwards) delta between surface and perceived surface
        var surfaceAngleDelta = Mathf.Abs(90f - Vector3.Angle(
            c.State.Curr.WallSurface.Normal,
            c.State.Curr.PerceivedSurface.Normal
        ));
        var surfaceAngleScale = 1f - (surfaceAngleDelta / 90f);

        // add wall gravity
        // AAA: consider whether we want to keep the adsr
        var wallGravityAmplitudeScale = c.Tuning.WallGravityAmplitudeScale.Evaluate(surfaceAngleScale);
        var wallGravity = c.Input.IsWallHoldPressed
            ? c.Tuning.WallHoldGravity
            : c.Tuning.WallGravity;

        // scale by wall angle
        var wallAngleScale = c.Tuning.WallAngleScale.Evaluate(wall.Angle);
        var wallAcceleration = c.Tuning.WallAcceleration(wallGravity);
        acceleration += wallAcceleration * wallAngleScale * wallUp;

        // update state
        c.State.Inertia -= inertiaDecay;
        c.State.Acceleration += acceleration;
    }
}

}