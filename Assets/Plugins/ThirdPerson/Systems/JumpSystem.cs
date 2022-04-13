using UnityEngine;

namespace ThirdPerson {

/// how the character jumps
sealed class JumpSystem: CharacterSystem {
    // -- props --
    /// the number of coyote frames the available
    int m_CoyoteFrames = 0;

    // -- lifetime --
    public JumpSystem(CharacterData character)
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
        name: "NotJumping",
        update: NotJumping_Update
    );

    void NotJumping_Update() {
        if (m_State.IsGrounded && m_Input.IsJumpDown(m_Tunables.JumpBuffer)) {
            ChangeTo(JumpSquat);
            return;
        }

        if (!m_State.IsGrounded && m_State.VerticalSpeed < 0.0f)  {
            m_CoyoteFrames = (int)m_Tunables.MaxCoyoteFrames;
            ChangeTo(Falling);
            return;
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
        m_State.JumpSquatFrame  = 0;
    }

    void JumpSquat_Update() {
        // apply fall acceleration if not grounded
        if (!m_State.IsGrounded && !m_State.IsOnWall) {
            m_State.VerticalSpeed += m_Tunables.FallAcceleration * Time.deltaTime;
        }

        // jump if jump was released or jump squat ended
        var shouldJump = (
            // if the jump squat finished
            m_State.JumpSquatFrame >= m_Tunables.MaxJumpSquatFrames ||
            // or jump was released after the minimum
            (!m_Input.IsJumpPressed && m_State.JumpSquatFrame >= m_Tunables.MinJumpSquatFrames)
        );

        if (shouldJump) {
            ChangeTo(Jumping);
            return;
        }

        // count coyote frames; reset to max whenever grounded
        if (m_State.IsGrounded) {
            m_CoyoteFrames = (int)m_Tunables.MaxCoyoteFrames;
        }
        // but if not, subtract a frame
        else {
            m_CoyoteFrames -= 1;
        }

        // fall once coyote time expires
        if (m_CoyoteFrames < 0) {
            ChangeTo(Falling);
            return;
        }

        // count jump squat frames
        m_State.JumpSquatFrame += 1;
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
            m_State.JumpSquatFrame
        );

        // interpolate initial jump speed
        var verticalSpeed = Mathf.Lerp(
            m_Tunables.MinJumpSpeed,
            m_Tunables.MaxJumpSpeed,
            m_Tunables.JumpSpeedCurve.Evaluate(pct)
        );

        var planarSpeed = Mathf.Lerp(
            m_Tunables.MinJumpSpeed_Horizontal,
            m_Tunables.MaxJumpSpeed_Horizontal,
            m_Tunables.JumpSpeedCurve_Horizontal.Evaluate(pct)
        );

        // cancel downwards momentum and apply initial jump
        m_State.VerticalSpeed = Mathf.Max(m_State.VerticalSpeed, 0.0f) + verticalSpeed;
        m_State.SetProjectedPlanarVelocity(m_State.PlanarVelocity + m_State.FacingDirection * planarSpeed);
        m_State.IsInJumpStart = true;
    }

    void Jumping_Update() {
        // only in jump start first frame
        // TODO: reason about frames -- when should enter be called? what is frame one?
        m_State.IsInJumpStart = false;

        // apply jump acceleration while holding jump
        if (m_Input.IsJumpPressed) {
            m_State.VerticalSpeed += m_Tunables.JumpAcceleration * Time.deltaTime;
        }

        // transition out of jump
        if (m_State.IsGrounded) {
            ChangeTo(NotJumping);
            return;
        }

        if (m_State.VerticalSpeed < 0.0f) {
            m_CoyoteFrames = 0;
            ChangeTo(Falling);
            return;
        }
    }

    // -- Falling --
    CharacterPhase Falling => new CharacterPhase(
        name: "Falling",
        update: Falling_Update
    );

    void Falling_Update() {
        // apply fall acceleration while holding jump
        // TODO: is this bad?
        if (m_Input.IsJumpPressed && !m_State.IsOnWall) {
            m_State.VerticalSpeed += m_Tunables.FallAcceleration * Time.deltaTime;
        }

        // count coyote frames
        m_CoyoteFrames -= 1;

        // start jump if jump is pressed before coyote frames expire
        // NOTE: it's possible to start a jump you can't or don't finish and spend
        // a few frames in jump squat before falling again
        // NOTE: we could sorta fix this by skipping jump squat, requiring the whole
        // jump finish here, and transitioning directly to jump
        if (m_CoyoteFrames >= 0 && m_Input.IsJumpDown()) {
            ChangeTo(JumpSquat);
            return;
        }

        // transition out of jump
        if (m_State.IsGrounded) {
            ChangeTo(NotJumping);
            return;
        }
    }

    // -- commands --
    void AddGravity() {
        m_State.VerticalSpeed += m_Tunables.Gravity * Time.deltaTime;
    }
}

}