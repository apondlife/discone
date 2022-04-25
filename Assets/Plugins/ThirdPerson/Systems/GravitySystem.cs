using UnityEngine;

namespace ThirdPerson {

/// how the character is affected by gravity
sealed class GravitySystem: CharacterSystem {
    // -- lifetime --
    public GravitySystem(CharacterData character)
        : base(character) {
    }

    protected override CharacterPhase InitInitialPhase() {
        return Grounded;
    }

    // -- Grounded --
    CharacterPhase Grounded => new CharacterPhase(
        name: "Grounded",
        update: Grounded_Update
    );

    void Grounded_Update() {
        if (!m_State.IsGrounded) {
            ChangeTo(Airborne);
        }

        // the normal force of the floor
        var v = m_State.Velocity;
        v += m_Tunables.Gravity * Time.deltaTime * Vector3.up;
        v += -Vector3.Project(m_Tunables.Gravity * Time.deltaTime * Vector3.up, m_State.GroundCollision.Normal);
        // v += Vector3.Project(v, m_State.GroundCollision.Normal);
        // tiny force that keeps you grounded;
        v -= 0.1f * m_State.GroundCollision.Normal;

        m_State.Velocity = v;
        Debug.Log($"We are grounded {m_State.Velocity.y}");

        SetGrounded();
    }

    // -- Airborne --
    CharacterPhase Airborne => new CharacterPhase(
        name: "Airborne",
        update: Airborne_Update
    );

    void Airborne_Update() {
        if (m_State.IsGrounded) {
            ChangeTo(Grounded);
        }

        m_State.Velocity += m_Tunables.Gravity * Time.deltaTime * Vector3.up;
        Debug.Log($"We are flying {m_State.Velocity.y}");
        SetGrounded();
    }

    // -- commands --
    void SetGrounded() {
        m_State.IsGrounded = m_Controller.IsGrounded;
    }
}

}