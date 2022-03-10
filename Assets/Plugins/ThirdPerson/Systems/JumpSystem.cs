using UnityEngine;

namespace ThirdPerson {

/// how the character jumps
sealed class JumpSystem: CharacterSystem {
    // -- props --
    /// the current frame in coyote time
    int m_CoyoteFrame = 0;

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
        if (m_State.IsGrounded && m_Input.IsJumpDown(m_Tunables.JumpBuffer)) {
            ChangeTo(JumpSquat);
            return;
        }

        if (!m_State.IsGrounded && m_State.VerticalSpeed < 0.0f)  {
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

        // cancel the jump if falling & coyote time expires
        if (m_State.IsGrounded) {
            m_CoyoteFrame = 0;
        } else {
            // this here is a weird moment, where you are falling, but not in the falling phase
            m_CoyoteFrame += 1;
        }

        if (m_CoyoteFrame > m_Tunables.MaxCoyoteFrames) {
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

        pct = m_Tunables.JumpSpeedCurve.Evaluate(pct);

        // interpolate initial jump speed
        var vt = Mathf.Lerp(
            m_Tunables.MinJumpSpeed,
            m_Tunables.MaxJumpSpeed,
            pct
        );

        // cancel vertical momentum and apply initial jump
        m_State.VerticalSpeed = vt;
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
            ChangeTo(Falling);
            return;
        }
    }

    // -- Falling --
    CharacterPhase Falling => new CharacterPhase(
        name: "Falling",
        enter: Falling_Enter,
        update: Falling_Update
    );

    void Falling_Enter() {
        m_CoyoteFrame = 0;
    }

    void Falling_Update() {
        // apply fall acceleration while holding jump
        // TODO: is this bad?
        if (m_Input.IsJumpPressed && !m_State.IsOnWall) {
            m_State.VerticalSpeed += m_Tunables.FallAcceleration * Time.deltaTime;
        }

        // add coyote frames
        m_CoyoteFrame += 1;

        // start jump if jump is pressed before coyote frames expire
        // NOTE: it's possible to start a jump you can't or don't finish and spend
        // a few frames in jump squat before falling again
        // NOTE: we could sorta fix this by skipping jump squat, requiring the whole
        // jump finish here, and transitioning directly to jump
        if (m_CoyoteFrame <= m_Tunables.MaxCoyoteFrames && m_Input.IsJumpDown()) {
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