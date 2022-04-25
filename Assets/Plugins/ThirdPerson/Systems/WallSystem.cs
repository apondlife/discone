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
        // if there's no hit, do nothing
        if (m_Controller.Collisions.Count == 0) {
            return;
        }

        // if we're on a wall, enter slide
        if (IsOnWall()) {
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
        // find wall collision
        var _ = FindWall(out var collision);

        // calculate initial slide velocity
        var transferred = FindTransferredVelocity(collision.Normal);
        m_State.Velocity += transferred.magnitude * Vector3.up;

        // update state
        m_State.IsOnWall = true;
    }

    void WallSlide_Update() {
        // if we left the wall, exit
        var isOnWall = FindWall(out var collision);
        if (!isOnWall) {
            ChangeTo(NotOnWall);
            return;
        }

        // transfer velocity
        var transferred = FindTransferredVelocity(collision.Normal);
        m_State.Velocity += transferred.magnitude * Vector3.up;

        // accelerate while holding button
        if (m_Input.IsHoldingWall) {
            m_State.Velocity += m_Tunables.WallAcceleration * Time.deltaTime * Vector3.up;
        }
    }

    // -- queries --
    /// if the character is on a wall this frame
    bool IsOnWall() {
        return FindWall(out _);
    }

    /// the collision with the wall this frame
    bool FindWall(out CharacterCollision collision) {
        // for each collision
        foreach (var c in m_Controller.Collisions) {
            // check the angle between the normal and up
            var angle = Vector3.Angle(c.Normal, Vector3.up);

            // if it's within the wall range, we're on a wall
            if (Mathf.Abs(90.0f - angle) <= (90.0f - m_Controller.WallAngle)) {
                collision = c;
                return true;
            }
        }

        collision = default;
        return false;
    }

    /// find the velocity to transfer to the wall
    Vector3 FindTransferredVelocity(Vector3 normal) {
        normal = Vector3.ProjectOnPlane(normal, Vector3.up).normalized;
        var transferred = Vector3.Project(m_State.GetFrame(1).Velocity.XNZ(), normal);
        return transferred;
    }
}

}