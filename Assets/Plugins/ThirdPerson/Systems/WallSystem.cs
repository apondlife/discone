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

        var lastHit = m_Controller.Collisions[m_Controller.Collisions.Count - 1];
        var angle = Mathf.Abs(Vector3.Dot(lastHit.Normal, Vector3.up));
        if (angle < 0.2f) {
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
        m_State.IsOnWall = true;
        var lastHit = m_Controller.Collisions[m_Controller.Collisions.Count - 1];
        var planarNormal = Vector3.ProjectOnPlane(lastHit.Normal, Vector3.up);
        var projectedVelocity = Vector3.Project(m_State.PrevPlanarVelocity, planarNormal);
        m_State.VerticalSpeed += projectedVelocity.magnitude;
        // Debug.Log($"wall-slide: n={m_State.Hit.Value.normal} n_p={planarNormal} v_0={m_State.PrevPlanarVelocity} v_n={projectedVelocity} dvy={2.0f * projectedVelocity.magnitude}");
    }

    void WallSlide_Update() {
         // if there's no hit, do nothing
        if (m_Controller.Collisions.Count == 0) {
            ChangeTo(NotOnWall);
            return;
        }


        var lastHit = m_Controller.Collisions[m_Controller.Collisions.Count - 1];
        var angle = Mathf.Abs(Vector3.Dot(lastHit.Normal, Vector3.up));
        if (angle > 0.2f) {
            ChangeTo(NotOnWall);
            return;
        }

        var planarNormal = Vector3.ProjectOnPlane(lastHit.Normal, Vector3.up);
        var projectedVelocity = Vector3.Project(m_State.PrevPlanarVelocity, planarNormal);
        m_State.VerticalSpeed += projectedVelocity.magnitude;

        if(m_Input.IsHoldingWall) {
            m_State.VerticalSpeed += m_Tunables.WallAcceleration * Time.deltaTime;
        }
    }
}

}