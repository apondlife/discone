using UnityEngine;

sealed class JumpSystem: CharacterSystem {
    // -- props --
    /// the number of frames left in jump squat
    int m_JumpSquatFrame = 0;

    // -- lifetime --
    public JumpSystem(Character character)
        : base(character) {
    }

    protected override CharacterPhase InitInitialPhase() {
        return NotJumping;
    }

    // -- lifecycle --
    public override void Update() {
        base.Update();

        // always add gravity
        AddGravity();
    }

    // -- NotJumping --
    CharacterPhase NotJumping => new CharacterPhase(
        name: "Jump_NotJumping",
        update: NotJumping_Update
    );

    void NotJumping_Update() {
        if (m_Input.IsJumpPressed && m_State.IsGrounded) {
            ChangeTo(JumpSquat);
        } else if (m_State.VerticalSpeed < 0.0f) {
            ChangeTo(Falling);
        }
    }

    // -- JumpSquat --
    CharacterPhase JumpSquat => new CharacterPhase(
        name: "JumpSquat",
        enter: JumpSquat_Enter,
        update: JumpSquat_Update,
        exit: JumpSquat_Exit
    );

    void JumpSquat_Enter() {
        m_State.IsInJumpSquat = true;
        m_JumpSquatFrame = 0;
    }

    void JumpSquat_Update() {
        var shouldJump = (
            // if the jump squat finished
            m_JumpSquatFrame >= m_Tunables.MaxJumpSquatFrames ||
            // or jump was released after the minimum
            (!m_Input.IsJumpPressed && m_JumpSquatFrame >= m_Tunables.MinJumpSquatFrames)
        );

        // if jump squat is done, transition to jump
        if (shouldJump) {
            ChangeTo(Jumping);
        }

        // count jump squat frames
        m_JumpSquatFrame += 1;
    }

    void JumpSquat_Exit() {
        m_State.IsInJumpSquat = false;
    }

    // -- Jumping --
    CharacterPhase Jumping => new CharacterPhase(
        name: "Jumping",
        enter: Jumping_Enter,
        update: Jumping_Update
    );

    void Jumping_Enter() {
        // get curved percent complete through jump squat
        var pct = Mathf.InverseLerp(
            m_Tunables.MinJumpSquatFrames,
            m_Tunables.MaxJumpSquatFrames,
            m_JumpSquatFrame
        );

        pct = m_Tunables.JumpSpeedCurve.Evaluate(pct);

        // interpolate initial jump speed
        m_State.VerticalSpeed += Mathf.Lerp(
            m_Tunables.MinJumpSpeed,
            m_Tunables.MaxJumpSpeed,
            pct
        );
    }

    void Jumping_Update() {
        // apply jump acceleration while holding jump
        if(m_Input.IsJumpPressed) {
            m_State.VerticalSpeed += m_Tunables.JumpAcceleration * Time.deltaTime;
        }

        // transition out of jump
        if (m_State.IsGrounded) {
            ChangeTo(NotJumping);
        } else if (m_State.VerticalSpeed < 0.0f) {
            ChangeTo(Falling);
        }
    }

    // -- Falling --
    CharacterPhase Falling => new CharacterPhase(
        name: "Falling",
        update: Falling_Update
    );

    void Falling_Update() {
        // apply fall acceleration while holding jump
        if(m_Input.IsJumpPressed) {
            m_State.VerticalSpeed += m_Tunables.FallAcceleration * Time.deltaTime;
        }

        // transition out of jump
        if (m_State.IsGrounded) {
            ChangeTo(NotJumping);
        }
    }

    // -- commands --
    void AddGravity() {
        m_State.VerticalSpeed += m_Tunables.Gravity * Time.deltaTime;
    }
}