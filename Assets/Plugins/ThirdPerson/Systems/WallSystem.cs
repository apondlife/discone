using UnityEngine;

namespace ThirdPerson {

/// how the character interacts with walls
sealed class WallSystem: CharacterSystem {
    // -- lifetime --
    public WallSystem(CharacterData character)
        : base(character) {
    }

    protected override CharacterPhase InitInitialPhase() {
        return NotOnWall;
    }

    // -- Grounded --
    CharacterPhase NotOnWall => new CharacterPhase(
        name: "NotOnWall",
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
        var wall = m_State.Prev.Wall;

        // transfer initial velocity
        var transferred = FindTransferredVelocity(wall.Normal);
        m_State.Velocity += transferred.magnitude * Vector3.up;

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

        // transfer velocity
        var transferred = FindTransferredVelocity(wall.Normal);

        var v = m_State.Velocity;
        v += transferred.magnitude * Vector3.up;
        v -= wall.Normal * m_Tunables.WallMagnet;

        // accelerate while holding button
        if (m_Input.IsHoldingWall) {
            v += m_Tunables.WallAcceleration * Time.deltaTime * Vector3.up;
        }

        // update state
        m_State.Velocity = v;
    }

    // -- queries --
    /// find the velocity to transfer to the wall
    Vector3 FindTransferredVelocity(Vector3 normal) {
        var projected = Vector3.ProjectOnPlane(normal, Vector3.up).normalized;
        var transferred = Vector3.Project(m_State.Prev.PlanarVelocity, projected);
        return transferred;
    }
}

}