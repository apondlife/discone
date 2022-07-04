using UnityEngine;

namespace ThirdPerson {

/// how the character jumps
sealed class JumpSystem: CharacterSystem {
    // -- props --
    /// the number of coyote frames the available
    int m_CoyoteFrames = 0;

    /// the number of jumps executed since last grounded
    int m_Jumps = 0;

    const int k_MaxJumps = 1;

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
        enter: NotJumping_Enter,
        update: NotJumping_Update
    );

    void NotJumping_Enter() {
        m_Jumps = 0;
    }

    void NotJumping_Update() {
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

        // if you jump
        if (m_Input.IsJumpDown(m_Tunables.JumpBuffer)) {
            ChangeTo(JumpSquat);
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
        m_State.JumpSquatFrame = 0;
    }

    void JumpSquat_Update() {
        // apply fall acceleration if not grounded
        if (!m_State.IsGrounded && !m_State.IsOnWall) {
            m_State.Velocity += m_Tunables.FallAcceleration * Time.deltaTime * Vector3.up;
        }

        // jump if jump was released or jump squat ended
        var shouldJump = (
            // if the jump squat finished
            m_State.JumpSquatFrame >= m_Tunables.MaxJumpSquatFrames ||
            // or jump was released after the minimum
            (!m_Input.IsJumpPressed && m_State.JumpSquatFrame >= m_Tunables.MinJumpSquatFrames)
        );

        if (shouldJump) {
            Jump();
            ChangeTo(Falling);
            return;
        }

        // if this is the first jump, you might be in coyote time
        if(m_Jumps == 0) {
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
        }

        // count jump squat frames
        m_State.JumpSquatFrame += 1;
    }

    void JumpSquat_Exit() {
        // NOTE: do we force the jump here?
        m_State.IsInJumpSquat = false;
    }

    // -- Falling --
    CharacterPhase Falling => new CharacterPhase(
        name: "Falling",
        enter: Falling_Enter,
        update: Falling_Update
    );

    void Falling_Enter() {
        // consume a jump whenever falling
        m_Jumps += 1;
    }

    void Falling_Update() {
        // apply fall acceleration while holding jump
        // TODO: is this bad?
        // apply jump acceleration while holding jump
        if (m_Input.IsJumpPressed && !m_State.IsOnWall) {
            var acceleration = m_State.Velocity.y > 0.0f
                ? m_Tunables.JumpAcceleration
                : m_Tunables.FallAcceleration;

            m_State.Velocity += acceleration * Time.deltaTime * Vector3.up;
        }

        // count coyote frames
        m_CoyoteFrames -= 1;

        // start jump if jump is pressed before coyote frames expire
        // a few frames in jump squat before falling again
        // NOTE: we could sorta fix this by skipping jump squat, requiring the whole
        // jump finish here, and transitioning directly to jump
        if (m_CoyoteFrames >= 0 && m_Input.IsJumpDown()) {
            ChangeTo(JumpSquat);
            return;
        }

        // start an air jump if available
        if (m_Jumps < k_MaxJumps && m_Input.IsJumpDown()) {
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
        m_State.Velocity += m_Tunables.Gravity * Time.deltaTime * Vector3.up;
    }

    void Jump() {
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
        var v = m_State.Velocity;
        v += (Mathf.Max(m_State.Velocity.y, 0.0f) + verticalSpeed) * Vector3.up;
        v += m_State.FacingDirection * planarSpeed;

        m_State.Velocity = v;
        m_State.IsInJumpStart = true;
    }
}

}