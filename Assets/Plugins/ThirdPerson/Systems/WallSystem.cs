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

        // TODO: add IEnumerable, or delete this struct
        var n = m_Controller.Collisions.Count;
        Debug.Log($"collision {n}");
        for (var i = 0; i < n; i++) {
            var collision = m_Controller.Collisions[i];
            Debug.Log($"collision.normal {collision.Normal}");
            var angle = Vector3.Angle(collision.Normal, Vector3.up);// Mathf.Abs(Vector3.Dot(lastHit.Normal, Vector3.up));
            var angleToWall = Mathf.Abs(angle - m_Controller.WallAngle);

            if (angleToWall < (90.0f - m_Controller.WallAngle)) {
                ChangeTo(WallSlide);
                break;
            }
        }
    }

    // -- WallSlide --
    CharacterPhase WallSlide => new CharacterPhase(
        name: "WallSlide",
        enter: WallSlide_Enter,
        update: WallSlide_Update
    );

    void WallSlide_Enter() {
        m_State.IsOnWall = true;
        var lastHit = m_Controller.Collisions.Last;
        var planarNormal = Vector3.ProjectOnPlane(lastHit.Normal, Vector3.up);
        var projectedVelocity = Vector3.Project(m_State.GetFrame(1).Velocity, planarNormal);
        m_State.VerticalSpeed += projectedVelocity.magnitude;
    }

    void WallSlide_Update() {
         // if there's no hit, do nothing
        if (m_Controller.Collisions.Count == 0) {
            ChangeTo(NotOnWall);
            return;
        }

        var lastHit = m_Controller.Collisions.Last;
        var angle = Mathf.Abs(Vector3.Dot(lastHit.Normal, Vector3.up));
        if (angle > 0.2f) {
            ChangeTo(NotOnWall);
            return;
        }

        var planarNormal = Vector3.ProjectOnPlane(lastHit.Normal, Vector3.up);
        var projectedVelocity = Vector3.Project(m_State.GetFrame(1).PlanarVelocity, planarNormal);
        m_State.VerticalSpeed += projectedVelocity.magnitude;

        if(m_Input.IsHoldingWall) {
            m_State.VerticalSpeed += m_Tunables.WallAcceleration * Time.deltaTime;
        }
    }
}

}