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
    // -- props --
    /// the current wall normal
    Vector3 m_WallNormal;

    /// the up vector projected onto the current wall
    Vector3 m_WallUp;

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
        enter: NotOnWall_Enter,
        update: NotOnWall_Update
    );

    void NotOnWall_Enter() {
        c.State.Next.IsOnWall = false;
    }

    void NotOnWall_Update(float delta) {
        // if we're on a wall, enter slide
        var wall = c.State.Curr.Wall;
        if (!wall.IsNone) {
            ChangeToImmediate(WallSlide, delta);
        }
    }

    // -- WallSlide --
    Phase WallSlide => new Phase(
        name: "WallSlide",
        enter: WallSlide_Enter,
        update: WallSlide_Update
    );

    void WallSlide_Enter() {
        // update state
        c.State.Next.IsOnWall = true;
    }

    void WallSlide_Update(float delta) {
        // if we left the wall, exit
        var wall = c.State.Curr.Wall;
        if (wall.IsNone) {
            ChangeTo(NotOnWall);
            return;
        }

        // update to new wall collision
        UpdateWall(wall);

        // transfer velocity
        var vd = Vector3.zero;
        vd += TransferredVelocity();
        vd -= m_WallNormal * c.Tuning.WallMagnet;

        // accelerate while holding button
        var wallGravity = c.Input.IsWallHoldPressed
            ? c.Tuning.WallHoldGravity.Evaluate(PhaseStart)
            : c.Tuning.WallGravity.Evaluate(PhaseStart);

        var wallAcceleration = c.Tuning.WallAcceleration(wallGravity);
        vd += wallAcceleration * delta * m_WallUp;

        // update state
        c.State.Velocity += vd;
    }

    // -- commands --
    /// update w/ the current wall collision
    void UpdateWall(CharacterCollision wall) {
        m_WallNormal = wall.Normal;
        m_WallUp = Vector3.ProjectOnPlane(Vector3.up, wall.Normal).normalized;
    }

    // -- queries --
    /// find the velocity transferred into the wall plane
    Vector3 TransferredVelocity() {
        // get the component of our velocity into the wall
        var velocity = c.State.Prev.Velocity;
        var velocityAlongWall = Vector3.ProjectOnPlane(velocity, m_WallNormal);
        var velocityIntoWall = velocity - velocityAlongWall;

        // and transfer it up the wall
        var transferMagnitude = velocityIntoWall.magnitude;
        var transferred = transferMagnitude * m_WallUp;

        return transferred;
    }
}

}