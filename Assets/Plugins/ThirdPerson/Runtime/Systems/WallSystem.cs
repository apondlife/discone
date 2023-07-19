using System;
using UnityEngine;

namespace ThirdPerson {

/// system state extensions
partial class CharacterState {
    partial class Frame {
        /// .
        public SystemState WallState;
    }
}

/// how the character interacts with walls
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

    // -- Grounded --
    Phase NotOnWall => new Phase(
        "NotOnWall",
        update: NotOnWall_Update
    );

    void NotOnWall_Update(float delta) {
        // if we're on a wall, enter slide
        var wall = c.State.Curr.WallSurface;
        var wallAngleScale = c.Tuning.WallAngleScale.Evaluate(wall.Angle);
        if (wallAngleScale > Mathf.Epsilon) {
            ChangeToImmediate(WallSlide, delta);
        }
    }

    // -- WallSlide --
    Phase WallSlide => new Phase(
        name: "WallSlide",
        update: WallSlide_Update
    );

    void WallSlide_Update(float delta) {
        // if we left the wall, exit
        var wall = c.State.Curr.WallSurface;
        var wallAngleScale = c.Tuning.WallAngleScale.Evaluate(wall.Angle);
        if (wallAngleScale <= Mathf.Epsilon) {
            ChangeTo(NotOnWall);
            return;
        }

        // update to new wall collision
        var wallNormal = wall.Normal;
        var wallUp = Vector3.ProjectOnPlane(Vector3.up, wall.Normal).normalized;
        var prevLastSurface = c.State.Prev.LastSurface;
        var wallTg = prevLastSurface.IsSome
            ? Vector3.ProjectOnPlane(prevLastSurface.Normal, wall.Normal).normalized
            : wallUp;

        // calculate added velocity
        var vd = Vector3.zero;
        // NOTE: unsure if we want to apply the magnet on things that are not "real"
        // walls, but we are for now on account of our principle of No Rules
        vd -= wallNormal * c.Tuning.WallMagnet;

        // AAA: normalize all this code
        // transfer velocity
        var deltaNormal = Vector3.Angle(
            c.State.Curr.WallSurface.Normal,
            c.State.Curr.PrevLastSurface.Normal
        );
        var normalAngleDelta = Mathf.Abs(90f - deltaNormal);
        var normalAngleScale = 1f - (normalAngleDelta / 90f);

        var wallTransferScale = c.Tuning.WallTransferScale.Evaluate(normalAngleScale);

        var transferred = TransferredVelocity(wallNormal, wallTg) * wallTransferScale;
        vd += transferred;

        if (deltaNormal >= Mathf.Epsilon) {
            Debug.Log($"[wallss] dn {deltaNormal} nad {normalAngleDelta} nas {normalAngleScale} wts {wallTransferScale} tra {transferred} tra ({transferred.magnitude})");
        }

        // accelerate while holding button
        var deltaAngle = Mathf.Abs(c.State.Curr.WallSurface.Angle - c.State.Curr.PrevLastSurface.Angle);
        var surfaceAngleDelta = Mathf.Abs(90f - deltaAngle);
        var surfaceAngleScale = 1f - (surfaceAngleDelta / 90f);

        var wallGravityAmplitudeScale = c.Tuning.WallGravityAmplitudeScale.Evaluate(surfaceAngleScale);
        var wallGravity = c.Input.IsWallHoldPressed
            ? c.Tuning.WallHoldGravity.Evaluate(PhaseStart, wallGravityAmplitudeScale)
            : c.Tuning.WallGravity.Evaluate(PhaseStart, wallGravityAmplitudeScale);

        if (deltaAngle >= Mathf.Epsilon) {
            Debug.Log($"[wallss] da {deltaAngle} sad {surfaceAngleDelta} sas {surfaceAngleScale} gas {wallGravityAmplitudeScale} g {wallGravity}");
        }

        var wallAcceleration = c.Tuning.WallAcceleration(wallGravity);
        vd += wallAcceleration * wallAngleScale * delta * wallUp;

        // scale acceleration by wall angle
        // vd *= wallAngleScale;

        // update state
        c.State.Velocity += vd;
    }

    // -- queries --
    /// find the velocity transferred into the wall plane
    Vector3 TransferredVelocity(
        Vector3 wallNormal,
        Vector3 wallUp
    ) {
        // get the component of our velocity into the wall
        var velocity = c.State.Prev.Velocity;
        var velocityAlongWall = Vector3.ProjectOnPlane(velocity, wallNormal);
        var velocityIntoWall = velocity - velocityAlongWall;

        // and transfer it up the wall
        var transferMagnitude = velocityIntoWall.magnitude;
        var transferred = transferMagnitude * wallUp;

        return transferred;
    }
}

}