using UnityEngine;

namespace ThirdPerson {

/// how the character jumps
sealed class JumpSystem: CharacterSystem {
    // -- props --
    /// the number of coyote frames the available
    int m_CoyoteFrames = 0;

    /// the number of cooldown frames available
    int m_CooldownFrames = 0;

    /// the current number of jumps in the current tunable
    uint m_JumpTunablesJumpIndex;

    /// the index of the current jump tunable
    uint m_JumpTunablesIndex;

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
        m_State.Curr.Velocity += m_Tunables.Gravity * Time.deltaTime * Vector3.up;
    }

    // -- NotJumping --
    CharacterPhase NotJumping => new CharacterPhase(
        name: "NotJumping",
        enter: NotJumping_Enter,
        update: NotJumping_Update
    );

    void NotJumping_Enter() {
        ResetJumps();
    }

    void NotJumping_Update() {
        // count coyote frames; reset to max whenever grounded
        if (m_State.Prev.IsGrounded) {
            m_CoyoteFrames = (int)m_Tunables.MaxCoyoteFrames;
        }
        // but if not, subtract a frame
        else {
            m_CoyoteFrames -= 1;
        }

        // fall once coyote time expires
        if (m_CoyoteFrames <= 0) {
            ChangeTo(Falling);
            return;
        }

        // if you jump
        if (CanJump() && m_Input.IsJumpDown(m_Tunables.JumpBuffer)) {
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
        m_State.Curr.IsInJumpSquat = true;
        m_State.Curr.JumpSquatFrame = 0;
    }

    void JumpSquat_Update() {
        // apply fall acceleration if not grounded
        if (!m_State.Prev.IsGrounded && !m_State.Curr.IsOnWall) {
            m_State.Curr.Velocity += m_Tunables.FallAcceleration * Time.deltaTime * Vector3.up;
        }

        // jump if jump was released or jump squat ended
        var shouldJump = (
            // if the jump squat finished
            m_State.Curr.JumpSquatFrame >= JumpTunables.MaxJumpSquatFrames ||
            // or jump was released after the minimum
            (!m_Input.IsJumpPressed && m_State.Curr.JumpSquatFrame >= JumpTunables.MinJumpSquatFrames)
        );

        if (shouldJump) {
            Jump();
            ChangeTo(Falling);
            return;
        }

        // if this is the first jump, you might be in coyote time
        if (m_JumpTunablesJumpIndex == 0) {
            // count coyote frames; reset to max whenever grounded
            if (m_State.Prev.IsGrounded) {
                m_CoyoteFrames = (int)m_Tunables.MaxCoyoteFrames;
            }
            // but if not, subtract a frame
            else {
                m_CoyoteFrames -= 1;
            }

            // fall once coyote time expires
            if (m_CoyoteFrames <= 0) {
                ChangeTo(Falling);
                return;
            }
        }

        // count jump squat frames
        m_State.Curr.JumpSquatFrame += 1;
    }

    void JumpSquat_Exit() {
        // NOTE: do we force the jump here?
        m_State.Curr.IsInJumpSquat = false;
    }

    // -- Falling --
    CharacterPhase Falling => new CharacterPhase(
        name: "Falling",
        enter: Falling_Enter,
        update: Falling_Update
    );

    void Falling_Enter() {
        IncrementJumps();
    }

    void Falling_Update() {
        // apply fall acceleration while holding jump
        // TODO: is this bad?
        // apply jump acceleration while holding jump
        if (m_Input.IsJumpPressed && !m_State.Curr.IsOnWall) {
            var acceleration = m_State.Curr.Velocity.y > 0.0f
                ? m_Tunables.JumpAcceleration
                : m_Tunables.FallAcceleration;

            m_State.Curr.Velocity += acceleration * Time.deltaTime * Vector3.up;
        }

        // count coyote frames
        m_CoyoteFrames -= 1;
        m_CooldownFrames -= 1;

        // start jump if jump is pressed before coyote frames expire
        // a few frames in jump squat before falling again
        // NOTE: we could sorta fix this by skipping jump squat, requiring the whole
        // jump finish here, and transitioning directly to jump
        if (CanJump() && m_Input.IsJumpDown()) {
            ChangeTo(JumpSquat);
            return;
        }

        // transition out of jump
        if (m_State.Prev.IsGrounded) {
            ChangeTo(NotJumping);
            return;
        }
    }

    // -- commands --
    void Jump() {
        // get curved percent complete through jump squat
        var pct = Mathf.InverseLerp(
            JumpTunables.MinJumpSquatFrames,
            JumpTunables.MaxJumpSquatFrames,
            m_State.Curr.JumpSquatFrame
        );

        // interpolate initial jump speed
        var verticalSpeed = Mathf.Lerp(
            JumpTunables.Vertical_MinSpeed,
            JumpTunables.Vertical_MaxSpeed,
            JumpTunables.Vertical_SpeedCurve.Evaluate(pct)
        );

        var horizontalSpeed = Mathf.Lerp(
            JumpTunables.Horizontal_MinSpeed,
            JumpTunables.Horizontal_MaxSpeed,
            JumpTunables.Horizontal_SpeedCurve.Evaluate(pct)
        );

        var v0 = m_State.Prev.Velocity;
        var dv = Vector3.zero;

        // cancel vertical momentum if falling.
        // according to tunables if going up
        // (we don't want to lose upwards speed in general, but not jumping if too fast is too weird)
        var verticalLoss = v0.y > 0 ? JumpTunables.Upwards_MomentumLoss : 1;
        dv -= v0.y * verticalLoss * Vector3.up;

        // cancel vertical momentum if falling
        // cancel horizontal momentum
        dv -= m_State.Prev.PlanarVelocity * JumpTunables.Horizontal_MomentumLoss;

        // add vertical jump
        dv += verticalSpeed * Vector3.up;

        // add horizontal jump
        dv += horizontalSpeed * m_State.Prev.Forward;

        m_State.Curr.Velocity += dv;
        m_State.Curr.IsInJumpStart = true;
        m_CooldownFrames = (int)JumpTunables.CooldownFrames;
    }

    /// track jump and switch to the correct jump if necessary
    void IncrementJumps() {
        m_State.Curr.Jumps += 1;
        m_JumpTunablesJumpIndex += 1;

        if (JumpTunables.Count == 0) {
            return;
        }

        var shouldAdvanceJump = (
            m_JumpTunablesJumpIndex >= JumpTunables.Count &&
            m_JumpTunablesIndex < m_Tunables.Jumps.Length - 1
        );

        if (shouldAdvanceJump) {
            m_JumpTunablesJumpIndex = 0;
            m_JumpTunablesIndex += 1;
        }
    }

    /// reset the jump count to its initial state
    void ResetJumps() {
        m_State.Curr.Jumps = 0;
        m_JumpTunablesJumpIndex = 0;
        m_JumpTunablesIndex = 0;
    }

    // -- queries --
    /// the current jump tunables
    CharacterTunablesBase.JumpTunablesBase JumpTunables {
        get => m_Tunables.Jumps[m_JumpTunablesIndex];
    }

    /// if this is the character's first (grounded) jump
    bool IsFirstJump {
        get => m_JumpTunablesIndex == 0 && m_JumpTunablesJumpIndex == 0;
    }

    /// if the character has a jump available to execute
    bool CanJump() {
        // if the character can't ever jump
        if (m_Tunables.Jumps.Length == 0) {
            return false;
        }

        // start jump if jump is pressed before coyote frames expire
        // a few frames in jump squat before falling again
        // NOTE: we could sorta fix this by skipping jump squat, requiring the whole
        // jump finish here, and transitioning directly to jump

        // if it's your first jump, account for coyote time
        if (IsFirstJump && m_CoyoteFrames >= 0) {
            return true;
        }

        if (m_CooldownFrames > 0) {
            return false;
        }

        // zero count means infinite jumps
        if (JumpTunables.Count == 0) {
            return true;
        }

        // start an air jump if available
        // if there's still jumps available in the current jump definition
        if (m_JumpTunablesJumpIndex < JumpTunables.Count) {
            return true;
        }

        return false;
    }
}

}