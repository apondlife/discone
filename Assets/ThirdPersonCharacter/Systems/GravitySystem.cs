using UnityEngine;

sealed class GravitySystem: CharacterSystem {
    // -- lifetime --
    public GravitySystem(Character character)
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

        AddGravity();
        SetGrounded();
    }

    // -- Airborne --
    CharacterPhase Airborne => new CharacterPhase(
        name: "Airborne",
        update: Airborne_Update
    );

    void Airborne_Update() {
        if (m_State.IsGrounded)
        {
            ChangeTo(Grounded);
        }

        AddGravity();
        SetGrounded();
    }

    // -- commands --
    void AddGravity() {
        m_State.VerticalSpeed += m_Tunables.Gravity * Time.deltaTime;
    }

    void SetGrounded() {
        m_State.IsGrounded = m_Controller.isGrounded;
    }
}