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

        // transfer velocity
        // NOTE: unsure if we want to apply the magnet on things that are not "real"
        // walls, but we are for now on account of our principle of No Rules
        var vd = Vector3.zero;
        vd += TransferredVelocity(wallNormal, wallUp);
        vd -= wallNormal * c.Tuning.WallMagnet;

        // accelerate while holding button
        // AAA: make sure that last surface is the last (not current) surface touched
        // TODO: fix AAA
        var deltaAngle = Mathf.Abs(c.State.Curr.Wall.Angle - c.State.Curr.LastSurface.Angle);
        var surfaceAngleDelta = Mathf.Abs(90f - deltaAngle);
        var surfaceAngleScale = 1f - (surfaceAngleDelta / 90f);

        var wallGravityAmplitudeScale = c.Tuning.WallGravityAmplitudeScale.Evaluate(surfaceAngleDelta);

        var wallGravity = c.Input.IsWallHoldPressed
            ? c.Tuning.WallHoldGravity.Evaluate(PhaseStart, wallGravityAmplitudeScale)
            : c.Tuning.WallGravity.Evaluate(PhaseStart, wallGravityAmplitudeScale);

        if (wallGravityAmplitudeScale < 1) {
            Debug.Log($"[wallss] sad {surfaceAngleDelta} sas {surfaceAngleScale} amplitude scale {wallGravityAmplitudeScale} grav {wallGravity}");
        }

        var wallAcceleration = c.Tuning.WallAcceleration(wallGravity);
        vd += wallAcceleration * delta * wallUp;

        // scale acceleration by wall angle
        vd *= wallAngleScale;

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