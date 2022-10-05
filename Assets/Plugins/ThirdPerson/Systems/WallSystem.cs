using System;
using UnityEngine;

namespace ThirdPerson {

/// how the character interacts with walls
[Serializable]
sealed class WallSystem: CharacterSystem {
    // -- props --
    /// the current wall normal
    Vector3 m_WallNormal;

    /// the up vector projected onto the current wall
    Vector3 m_WallUp;

    // -- lifetime --
    protected override Phase InitInitialPhase() {
        return NotOnWall;
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
        var wall = m_State.Prev.Wall;
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
        UpdateWall(m_State.Prev.Wall);

        // transfer initial velocity
        var vd = Vector3.zero;
        vd += TransferredVelocity();
        m_State.Velocity += vd;

        // update state
        m_State.IsOnWall = true;
    }

    void WallSlide_Update(float delta) {
        // if we left the wall, exit
        var wall = m_State.Prev.Wall;
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
        if (m_Input.IsHoldingWall) {
            vd += m_Tunables.WallAcceleration * delta * m_WallUp;
        }

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
        var transferred = velocityIntoWall.magnitude * m_WallUp;

        return transferred;
    }
}

}