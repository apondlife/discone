using UnityEngine;

sealed class JumpSystem: CharacterSystem {
    // -- props --
    /// the number of frames left in jump squat
    int m_JumpSquatFrames = 0;

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
        m_JumpSquatFrames = m_Tunables.JumpSquatFrames;
    }

    void JumpSquat_Update() {
        // count down frames until jump squat ends
        m_JumpSquatFrames -= 1;

        if (m_JumpSquatFrames <= 0 || !m_Input.IsJumpPressed) {
            ChangeTo(Jumping);
        }
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
            m_Tunables.JumpSquatFrames,
            0,
            m_JumpSquatFrames
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
        if(m_Input.IsJumpPressed) {
            m_State.VerticalSpeed += m_Tunables.FloatAcceleration * Time.deltaTime;
        }

        if (m_State.IsGrounded) {
            ChangeTo(NotJumping);
        }
    }

    void AddGravity() {
        m_State.VerticalSpeed += m_Tunables.Gravity * Time.deltaTime;
    }
}