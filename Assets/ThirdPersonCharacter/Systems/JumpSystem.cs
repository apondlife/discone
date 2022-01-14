using UnityEngine;

class JumpSystem: CharacterSystem {
    // -- lifetime --
    public JumpSystem(Character character)
        : base(character) {
    }

    protected override CharacterPhase InitInitialPhase() {
        return NotJumping;
    }

    // -- NotJumping --
    CharacterPhase NotJumping => new CharacterPhase(
        name: "Jump_NotJumping",
        update: NotJumping_Update
    );

    void NotJumping_Update() {
        if (m_Input.DesiresToJump && m_State.IsGrounded) {
            ChangeTo(Jumping);
        }
    }

    // -- Jumping --
    CharacterPhase Jumping => new CharacterPhase(
        name: "Jumping",
        enter: Jumping_Enter,
        update: Jumping_Update
    );

    void Jumping_Enter() {
        m_State.VerticalSpeed += m_Tunables.InitialJumpSpeed;
    }

    void Jumping_Update() {
        if (m_State.IsGrounded) {
            ChangeTo(NotJumping);
        }
    }
}