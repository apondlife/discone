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
        get => m_State.Next.WallState;
    }

    // -- Grounded --
    Phase NotOnWall => new Phase(
        "NotOnWall",
        enter: NotOnWall_Enter,
        update: NotOnWall_Update
    );

    void NotOnWall_Enter() {
        m_State.IsOnWall = false;
    }

    void NotOnWall_Update(float _) {
        // if we're on a wall, enter slide
        var wall = m_State.Curr.Wall;
        if (!wall.IsNone) {
            ChangeTo(WallSlide);
        }
    }

    // -- WallSlide --
    Phase WallSlide => new Phase(
        name: "WallSlide",
        enter: WallSlide_Enter,
        update: WallSlide_Update
    );

    void WallSlide_Enter() {
        // update to new wall collision
        UpdateWall(m_State.Curr.Wall);

        // transfer initial velocity
        var vd = Vector3.zero;
        vd += TransferredVelocity();
        m_State.Velocity += vd;

        // update state
        m_State.IsOnWall = true;
    }

    void WallSlide_Update(float delta) {
        // if we left the wall, exit
        var wall = m_State.Curr.Wall;
        if (wall.IsNone) {
            ChangeTo(NotOnWall);
            return;
        }

        // update to new wall collision
        UpdateWall(wall);

        // transfer velocity
        var vd = Vector3.zero;
        vd += TransferredVelocity();
        vd -= m_WallNormal * m_Tunables.WallMagnet;

        // accelerate while holding button
        var wallGravity = m_Input.IsWallHoldPressed
            ? m_Tunables.WallHoldGravity.Evaluate(PhaseStart)
            : m_Tunables.WallGravity.Evaluate(PhaseStart);

        var wallAcceleration = m_Tunables.WallAcceleration(wallGravity);
        vd += wallAcceleration * delta * m_WallUp;

        // update state
        m_State.Velocity += vd;
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
        var velocity = m_State.Prev.Velocity;
        var velocityAlongWall = Vector3.ProjectOnPlane(velocity, m_WallNormal);
        var velocityIntoWall = velocity - velocityAlongWall;

        // and transfer it up the wall
        var transferMagnitude = velocityIntoWall.magnitude;
        var transferred = transferMagnitude * m_WallUp;

        return transferred;
    }
}

}