using UnityEngine;

namespace ThirdPerson {

/// how the character interacts with walls
sealed class WallSystem: CharacterSystem {

    // -- lifetime --
    public WallSystem(Character character)
        : base(character) {
    }

    protected override CharacterPhase InitInitialPhase() {
        return NotOnWall;
    }

    // -- Grounded --
    CharacterPhase NotOnWall => new CharacterPhase(
        name: "NotOnWall",
        update: NotOnWall_Update
    );

    void NotOnWall_Update() {
        // if there's no hit, do nothing
        if (m_State.Hit == null) {
            return;
        }
        var hit = m_State.Hit.Value;

        // if the normal is not a wall, do nothing
        if (Mathf.Abs(Vector3.Dot(hit.normal.normalized, Vector3.up)) > 0.8f) {
            return;
        }

        // switch to wall slide if the layer is a wall
        if (m_Tunables.WallLayer.Contains(hit.otherCollider.gameObject.layer)) {
            ChangeTo(WallSlide);
            return;
        }
    }

    // -- WallSlide --
    CharacterPhase WallSlide => new CharacterPhase(
        name: "WallSlide",
        enter: WallSlide_Enter,
        update: WallSlide_Update
    );

    void WallSlide_Enter() {
        var planarNormal = Vector3.ProjectOnPlane(m_State.Hit.Value.normal, Vector3.up).normalized;
        var projectedVelocity = Vector3.Project(m_State.PrevPlanarVelocity, planarNormal);
        // m_State.PlanarVelocity += projectedVelocity;
        m_State.VerticalSpeed += 2.0f * projectedVelocity.magnitude;
        Debug.Log($"wall-slide: n={m_State.Hit.Value.normal} n_p={planarNormal} v_0={m_State.PrevPlanarVelocity} v_n={projectedVelocity} dvy={2.0f * projectedVelocity.magnitude}");
    }

    void WallSlide_Update() {
        if(m_State.PrevVerticalSpeed < 0) {
            // m_State.VerticalSpeed = 0.5f * m_State.PrevVerticalSpeed;
        }

        if (m_State.IsGrounded) {
            ChangeTo(NotOnWall);
            return;
        }
    }
}

}