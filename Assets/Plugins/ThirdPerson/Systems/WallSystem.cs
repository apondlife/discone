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
    protected override CharacterPhase InitInitialPhase() {
        return NotOnWall;
    }

    // -- Grounded --
    CharacterPhase NotOnWall => new CharacterPhase(
        "NotOnWall",
        enter: NotOnWall_Enter,
        update: NotOnWall_Update
    );

    void NotOnWall_Enter() {
        m_State.IsOnWall = false;
    }

    void NotOnWall_Update() {
        // if we're on a wall, enter slide
        var wall = m_State.Prev.Wall;
        if (!wall.IsNone) {
            ChangeTo(WallSlide);
        }
    }

    // -- WallSlide --
    CharacterPhase WallSlide => new CharacterPhase(
        name: "WallSlide",
        enter: WallSlide_Enter,
        update: WallSlide_Update
    );

    void WallSlide_Enter() {
        // update to new wall collision
        UpdateWall(m_State.Prev.Wall);

        // transfer initial velocity
        var vd = Vector3.zero;
        vd += TransferVelocity();
        m_State.Velocity += vd;

        // update state
        m_State.IsOnWall = true;
    }

    void WallSlide_Update() {
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
        vd += TransferVelocity();
        vd -= m_WallNormal * m_Tunables.WallMagnet;

        // accelerate while holding button
        if (m_Input.IsHoldingWall) {
            vd += m_Tunables.WallAcceleration * Time.deltaTime * m_WallUp;
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
    /// find the speed to transfer to the wall
    Vector3 TransferVelocity() {
        // get wall normal
        var velocity = m_State.Prev.PlanarVelocity;
        var velocityInRelationToWall = Vector3.ProjectOnPlane(velocity, m_WallNormal);

        // the speed in the wall's direction
        var transferSpeed = (velocity - velocityInRelationToWall).magnitude;

        return m_WallUp * transferSpeed;
    }
}

}