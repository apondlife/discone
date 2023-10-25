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

    public override void Update(float delta) {
        // apply a decay to the raw momentum
        // AAA: convert to a half life (maybe)
        // c.State.Next.Inertia -= c.State.Next.Inertia * (1f - c.Tuning.Surface_MomentumDecay);
        // c.State.Next.Velocity += momentum;

        base.Update(delta);
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

        // calculate added velocity
        var vd = Vector3.zero;

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
        // TODO: prefix surface tuning values w/ `Surface_<name>`
        // var wallMagnetInputScale = c.Tuning.WallMagnetInputScale.Evaluate(wallInputUp);
        // var wallMagnetTransferScale = c.Tuning.WallMagnetTransferScale.Evaluate(normalAngleScale);
        // var wallMagnetMag = c.Tuning.WallMagnet.Evaluate(wall.Angle) * wallMagnetInputScale * wallMagnetTransferScale;
        // vd -= wallMagnetMag * delta * wallNormal;

        // find surface-based transfer scale
        var transferDiAngle = Vector3.SignedAngle(wallSurfaceTg, wallInputTg, wallNormal);
        var transferDiAngleMag = Mathf.Abs(transferDiAngle);
        var transferDiAngleSign = Mathf.Sign(transferDiAngle);

        var transferDiRot = c.Tuning.WallTransferDiAngle.Evaluate(transferDiAngleMag) * transferDiAngleSign * c.Input.MoveMagnitude;
        var transferTg = Quaternion.AngleAxis(transferDiRot, wallNormal) * wallSurfaceTg;

        var transferDiScale = c.Tuning.WallTransferDiScale.Evaluate(transferDiAngleMag);
        // AAA: this should have a better name
        var transferScale = 1.0f;//c.Tuning.WallTransferScale.Evaluate(normalAngleScale);
        Debug.Log($"[inertia] wts {wallToSurface} -> nad {normalAngleDelta} -> nas {normalAngleScale} -> ts {transferScale}");

        // transfer inertia up new surface w/ di
        var inertia = c.State.Curr.Inertia;
        var inertiaTg = Vector3.ProjectOnPlane(inertia, wallNormal);
        var inertiaNormal = inertia - inertiaTg;
        var inertiaDecay = inertiaNormal * c.Tuning.Surface_InertiaDecayScale.Evaluate(wall.Angle);

        // and transfer it along the surface tangent
        var transferMagnitude = inertiaDecay.magnitude * transferScale * transferDiScale;
        var transferVelocity = transferMagnitude * transferTg;
        vd +=  transferVelocity;

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
            ? c.Tuning.WallHoldGravity.Evaluate(5f, wallGravityAmplitudeScale)
            : c.Tuning.WallGravity.Evaluate(5f, wallGravityAmplitudeScale);

        // scale by wall angle
        var wallAngleScale = c.Tuning.WallAngleScale.Evaluate(wall.Angle);
        var wallAcceleration = c.Tuning.WallAcceleration(wallGravity);
        vd += wallAcceleration * wallAngleScale * delta * wallUp;

        /// AAA: add to acceleration properly
        // update state
        c.State.Inertia -= inertiaDecay;
        c.State.Acceleration += vd / delta;
    }
}

}