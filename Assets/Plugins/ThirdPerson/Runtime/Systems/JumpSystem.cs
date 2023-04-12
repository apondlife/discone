using System;
using UnityEngine;

namespace ThirdPerson {

/// system state extensions
partial class CharacterState {
    partial class Frame {
        /// .
        public SystemState JumpState;
    }
}

/// how the character jumps
[Serializable]
sealed class JumpSystem: CharacterSystem {
    // -- System --
    protected override Phase InitInitialPhase() {
        return NotJumping;
    }

    protected override SystemState State {
        get => m_State.Next.JumpState;
        set => m_State.Next.JumpState = value;
    }

    // -- lifecycle --
    public override void Init() {
        base.Init();
        ResetJumps();
    }

    public override void Update(float delta) {
        base.Update(delta);

        // always add gravity
        m_State.Next.Velocity += m_Tunables.Gravity * delta * Vector3.up;
    }

    // -- NotJumping --
    Phase NotJumping => new Phase(
        name: "NotJumping",
        enter: NotJumping_Enter,
        update: NotJumping_Update
    );

    void NotJumping_Enter() {
        m_State.Next.IsLanding = false;
    }

    void NotJumping_Update(float _) {
        // count coyote frames; reset to max whenever (this determine's if the
        // character is grounded)
        if (IsOnGround()) {
            m_State.CoyoteFrames = (int)m_Tunables.MaxCoyoteFrames;
        }
        // but if not, subtract a frame
        else {
            m_State.CoyoteFrames -= 1;
        }

        // fall once coyote time expires
        if (m_State.CoyoteFrames <= 0) {
            ChangeTo(Falling);
            return;
        }

        // if you jump
        if (CanJump() && m_Input.IsJumpDown(m_Tunables.JumpBuffer)) {
            ChangeTo(JumpSquat);
            return;
        }
    }

    // -- Landing --
    Phase Landing => new Phase(
        name: "Landing",
        enter: Landing_Enter,
        update: Landing_Update
    );

    void Landing_Enter() {
        ResetJumps();
        m_State.Next.IsLanding = true;
    }

    void Landing_Update(float _) {
        // count coyote frames; reset to max whenever (this determine's if the
        // character is grounded)
        if (IsOnGround()) {
            m_State.CoyoteFrames = (int)m_Tunables.MaxCoyoteFrames;
        }
        // but if not, subtract a frame
        else {
            m_State.CoyoteFrames -= 1;
        }

        // fall once coyote time expires
        if (m_State.CoyoteFrames <= 0) {
            ChangeTo(Falling);
            return;
        }

        // if you jump
        if (CanJump() && m_Input.IsJumpDown(m_Tunables.JumpBuffer)) {
            ChangeTo(JumpSquat);
            return;
        }

        // once landing completes
        if (PhaseElapsed > m_Tunables.Landing_Duration) {
            ChangeTo(NotJumping);
            return;
        }
    }

    // -- JumpSquat --
    Phase JumpSquat => new Phase(
        name: "JumpSquat",
        enter: JumpSquat_Enter,
        update: JumpSquat_Update,
        exit: JumpSquat_Exit
    );

    void JumpSquat_Enter() {
        m_State.Next.IsInJumpSquat = true;
        m_State.Next.JumpSquatFrame = 0;
    }

    void JumpSquat_Update(float delta) {
        // apply fall acceleration if airborne
        if (m_State.Curr.Ground.IsNone && m_State.Curr.Wall.IsNone) {
            m_State.Next.Velocity += m_Tunables.FallAcceleration * delta * Vector3.up;
        }

        // jump if jump was released or jump squat ended
        var shouldJump = (
            // if the jump squat finished
            m_State.Next.JumpSquatFrame >= JumpTunables.MaxJumpSquatFrames ||
            // or jump was released after the minimum
            (!m_Input.IsJumpPressed && m_State.Next.JumpSquatFrame >= JumpTunables.MinJumpSquatFrames)
        );

        if (shouldJump) {
            Jump();
            ChangeTo(Falling);
            return;
        }

        // if this is the first jump, you might be in coyote time
        if (m_State.JumpTunablesJumpIndex == 0) {
            // count coyote frames; reset to max whenever grounded
            if (IsOnGround()) {
                m_State.CoyoteFrames = (int)m_Tunables.MaxCoyoteFrames;
            }
            // but if not, subtract a frame
            else {
                m_State.CoyoteFrames -= 1;
            }

            // fall once coyote time expires
            if (m_State.CoyoteFrames <= 0) {
                ChangeTo(Falling);
                return;
            }
        }

        // count jump squat frames
        m_State.Next.JumpSquatFrame += 1;
    }

    void JumpSquat_Exit() {
        // NOTE: do we force the jump here?
        m_State.Next.IsInJumpSquat = false;
    }

    // -- Falling --
    Phase Falling => new Phase(
        name: "Falling",
        enter: Falling_Enter,
        update: Falling_Update
    );

    void Falling_Enter() {
        IncrementJumps();
        m_State.Next.IsLanding = false;
    }

    void Falling_Update(float delta) {
        // apply fall acceleration while holding jump
        // TODO: is this bad? yes?
        // apply jump acceleration while holding jump
        if (m_Input.IsJumpPressed && !m_State.Next.IsOnWall) {
            var acceleration = m_State.Next.Velocity.y > 0.0f
                ? m_Tunables.JumpAcceleration
                : m_Tunables.FallAcceleration;

            m_State.Next.Velocity += acceleration * delta * Vector3.up;
        }

        // count coyote frames
        m_State.CoyoteFrames -= 1;
        m_State.CooldownFrames -= 1;

        // start jump if jump is pressed before coyote frames expire
        // a few frames in jump squat before falling again
        // NOTE: we could sorta fix this by skipping jump squat, requiring the whole
        // jump finish here, and transitioning directly to jump
        if (CanJump() && m_Input.IsJumpDown()) {
            ChangeTo(JumpSquat);
            return;
        }

        // transition out of jump
        if (IsOnGround()) {
            ChangeTo(Landing);
            return;
        }
    }

    // -- commands --
    void Jump() {
        // get curved percent complete through jump squat
        var pct = Mathf.InverseLerp(
            JumpTunables.MinJumpSquatFrames,
            JumpTunables.MaxJumpSquatFrames,
            m_State.Next.JumpSquatFrame
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

        var v0 = m_State.Curr.Velocity;
        var dv = Vector3.zero;

        // cancel vertical momentum if falling.
        // according to tunables if going up
        // (we don't want to lose upwards speed in general, but not jumping if too fast is too weird)
        var verticalLoss = v0.y > 0 ? JumpTunables.Upwards_MomentumLoss : 1;
        dv -= v0.y * verticalLoss * Vector3.up;

        // cancel horizontal momentum
        dv -= m_State.Curr.PlanarVelocity * JumpTunables.Horizontal_MomentumLoss;

        // scale by wall factor
        // TODO: maybe horizontal/vertical should be tangent/normal to ground or wall:
        var groundAngleScale = m_Tunables.Jump_GroundAngleScale.Evaluate(m_State.Curr.GroundSurface.Angle);

        // add vertical jump
        dv += verticalSpeed * Vector3.up * groundAngleScale;

        // add horizontal jump
        dv += horizontalSpeed * m_State.Curr.Forward * groundAngleScale;

        m_State.Next.Velocity += dv;
        m_State.Next.CoyoteFrames = 0;
        m_State.Next.CooldownFrames = (int)JumpTunables.CooldownFrames;

        m_Events.Schedule(CharacterEvent.Jump);
    }

    /// track jump and switch to the correct jump if necessary
    void IncrementJumps() {
        m_State.Next.Jumps += 1;
        m_State.JumpTunablesJumpIndex += 1;

        if (JumpTunables.Count == 0) {
            return;
        }

        var shouldAdvanceJump = (
            m_State.JumpTunablesJumpIndex >= JumpTunables.Count &&
            m_State.JumpTunablesIndex < m_Tunables.Jumps.Length - 1
        );

        if (shouldAdvanceJump) {
            m_State.JumpTunablesJumpIndex = 0;
            m_State.JumpTunablesIndex += 1;
        }
    }

    /// reset the jump count to its initial state
    void ResetJumps() {
        m_State.Next.Jumps = 0;
        m_State.JumpTunablesJumpIndex = 0;
        m_State.JumpTunablesIndex = 0;
    }

    // -- queries --
    /// the current jump tunables
    CharacterTunablesBase.JumpTunablesBase JumpTunables {
        get => m_Tunables.Jumps[m_State.JumpTunablesIndex];
    }

    /// if this is the character's first (grounded) jump
    bool IsFirstJump {
        get => m_State.JumpTunablesIndex == 0 && m_State.JumpTunablesJumpIndex == 0;
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
        if (IsFirstJump && m_State.CoyoteFrames >= 0) {
            return true;
        }

        if (m_State.CooldownFrames > 0) {
            return false;
        }

        // zero count means infinite jumps
        if (JumpTunables.Count == 0) {
            return true;
        }

        // start an air jump if available
        // if there's still jumps available in the current jump definition
        if (m_State.JumpTunablesJumpIndex < JumpTunables.Count) {
            return true;
        }

        return false;
    }

    /// if the character is on something ground like
    bool IsOnGround() {
        var ground = m_State.Curr.GroundSurface;
        if (ground.IsNone) {
            return false;
        }

        return m_Tunables.Jump_GroundAngleScale.Evaluate(ground.Angle) > Mathf.Epsilon;
    }
}

}