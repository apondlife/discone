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
        get => c.State.Next.JumpState;
        set => c.State.Next.JumpState = value;
    }

    // -- lifecycle --
    public override void Init() {
        base.Init();
        ResetJumps();
    }

    public override void Update(float delta) {
        base.Update(delta);

        // always add gravity
        c.State.Next.Force += c.Tuning.Gravity * Vector3.up;
    }

    // -- NotJumping --
    Phase NotJumping => new(
        name: "NotJumping",
        enter: NotJumping_Enter,
        update: NotJumping_Update
    );

    void NotJumping_Enter() {
        c.State.Next.IsLanding = false;
    }

    void NotJumping_Update(float _) {
        // reset jump surface whenever grounded
        if (IsOnGround()) {
            ResetJumpSurface();
        }
        // but if not, subtract a frame
        else {
            c.State.CoyoteFrames -= 1;
        }

        // fall once coyote time expires
        if (c.State.CoyoteFrames <= 0) {
            ChangeTo(Falling);
            return;
        }

        // if you jump
        if (ShouldStartJump(c.Tuning.JumpBuffer)) {
            ChangeTo(JumpSquat);
            return;
        }
    }

    // -- Landing --
    Phase Landing => new(
        name: "Landing",
        enter: Landing_Enter,
        update: Landing_Update
    );

    void Landing_Enter() {
        ResetJumps();
        c.State.Next.IsLanding = true;
        c.Events.Schedule(CharacterEvent.Land);
    }

    void Landing_Update(float _) {
        // reset jump surface whenever grounded
        if (IsOnGround()) {
            ResetJumpSurface();
        }
        // but if not, subtract a frame
        else {
            c.State.CoyoteFrames -= 1;
        }

        // fall once coyote time expires
        if (c.State.CoyoteFrames <= 0) {
            ChangeTo(Falling);
            return;
        }

        // if you jump
        if (ShouldStartJump(c.Tuning.JumpBuffer)) {
            ChangeTo(JumpSquat);
            return;
        }

        // once landing completes
        if (PhaseElapsed > c.Tuning.Landing_Duration) {
            ChangeTo(NotJumping);
            return;
        }
    }

    // -- JumpSquat --
    Phase JumpSquat => new(
        name: "JumpSquat",
        enter: JumpSquat_Enter,
        update: JumpSquat_Update,
        exit: JumpSquat_Exit
    );

    void JumpSquat_Enter() {
        c.State.Next.IsInJumpSquat = true;
        c.State.Next.JumpSquatFrame = 0;
    }

    void JumpSquat_Update(float delta) {
        // apply fall acceleration if airborne
        if (c.State.Curr.MainSurface.IsNone) {
            c.State.Next.Force += c.Tuning.FallAcceleration * Vector3.up;
        }

        // jump if jump was released or jump squat ended
        var shouldJump = (
            // if the jump squat finished
            c.State.Next.JumpSquatFrame >= JumpTuning.MaxJumpSquatFrames ||
            // or jump was released after the minimum
            (!c.Input.IsJumpPressed && c.State.Next.JumpSquatFrame >= JumpTuning.MinJumpSquatFrames)
        );

        if (shouldJump) {
            Jump();
            ChangeTo(Falling);
            return;
        }

        // if this is the first jump, you might be in coyote time
        if (c.State.JumpTuningJumpIndex == 0) {
            // reset jump surface whenever grounded
            if (IsOnGround()) {
                ResetJumpSurface();
            }
            // but if not, subtract a frame
            else {
                c.State.CoyoteFrames -= 1;
            }

            // fall once coyote time expires
            if (c.State.CoyoteFrames <= 0) {
                ChangeTo(Falling);
                return;
            }
        }

        // count jump squat frames
        c.State.Next.JumpSquatFrame += 1;
    }

    void JumpSquat_Exit() {
        // NOTE: do we force the jump here?
        c.State.Next.IsInJumpSquat = false;
    }

    // -- Falling --
    Phase Falling => new(
        name: "Falling",
        enter: Falling_Enter,
        update: Falling_Update
    );

    void Falling_Enter() {
        IncrementJumps();
        c.State.Next.IsLanding = false;
    }

    void Falling_Update(float delta) {
        // apply fall acceleration while holding jump in the air
        if (c.Input.IsJumpPressed && c.State.Curr.MainSurface.IsNone) {
            var acceleration = c.State.Curr.Velocity.y > 0.0f
                ? c.Tuning.JumpAcceleration // TODO: what if you are going up but not from a jump?
                : c.Tuning.FallAcceleration;

            c.State.Next.Force += acceleration * Vector3.up;
        }

        // count coyote frames
        c.State.CoyoteFrames -= 1;
        c.State.CooldownFrames -= 1;

        // start jump on a new press
        if (ShouldStartJump()) {
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
    /// reset the next surface to jump from
    void ResetJumpSurface() {
        c.State.CoyoteFrames = c.Tuning.MaxCoyoteFrames;
        c.State.JumpSurface = c.State.Next.MainSurface;
    }

    /// .
    void Jump() {
        // accumulate jump delta dv
        var v0 = c.State.Curr.Velocity;
        var dv = Vector3.zero;

        // cancel vertical momentum if falling.
        // according to tuning if going up
        // (we don't want to lose upwards speed in general, but not jumping if too fast is too weird)
        var verticalLoss = v0.y > 0 ? JumpTuning.Upwards_MomentumLoss : 1;
        dv -= v0.y * verticalLoss * Vector3.up;

        // cancel horizontal momentum
        dv -= c.State.Curr.PlanarVelocity * JumpTuning.Horizontal_MomentumLoss;

        // get curved percent complete through jump squat
        var pct = Mathf.InverseLerp(
            JumpTuning.MinJumpSquatFrames,
            JumpTuning.MaxJumpSquatFrames,
            c.State.Next.JumpSquatFrame
        );

        // add directional jump velocity
        // TODO: change Vector3.up to JumpTuning.Direction
        var jumpSpeed = Mathf.Lerp(
            JumpTuning.Vertical_MinSpeed,
            JumpTuning.Vertical_MaxSpeed,
            JumpTuning.Vertical_SpeedCurve.Evaluate(pct)
        );

        var jumpSurface = c.State.JumpSurface;
        var jumpAngleScale = c.Tuning.Jump_SurfaceAngleScale.Evaluate(jumpSurface.Angle);
        dv += jumpSpeed * jumpAngleScale * Vector3.up;

        // add surface normal jump velocity
        if (c.State.PerceivedSurface.IsSome) {
            var normalSpeed = c.Tuning.Jump_Normal_Speed.Evaluate(pct);
            var normalSurface = c.State.Curr.PerceivedSurface;
            var normalAngleScale = c.Tuning.Jump_Normal_SurfaceAngleScale.Evaluate(normalSurface.Angle);
            dv += normalSpeed * normalAngleScale * normalSurface.Normal;
        }

        // update state
        c.State.Next.Inertia = 0f;
        c.State.Next.Velocity += dv;
        c.State.Next.CoyoteFrames = 0;
        c.State.Next.CooldownFrames = JumpTuning.CooldownFrames;
        c.Events.Schedule(CharacterEvent.Jump);
    }

    /// track jump and switch to the correct jump if necessary
    void IncrementJumps() {
        c.State.Next.Jumps += 1;
        c.State.JumpTuningJumpIndex += 1;

        if (JumpTuning.Count == 0) {
            return;
        }

        var shouldAdvanceJump = (
            c.State.JumpTuningJumpIndex >= JumpTuning.Count &&
            c.State.JumpTuningIndex < c.Tuning.Jumps.Length - 1
        );

        if (shouldAdvanceJump) {
            c.State.JumpTuningJumpIndex = 0;
            c.State.JumpTuningIndex += 1;
        }
    }

    /// reset the jump count to its initial state
    void ResetJumps() {
        c.State.Next.Jumps = 0;
        c.State.JumpTuningJumpIndex = 0;
        c.State.JumpTuningIndex = 0;
    }

    // -- queries --
    /// the current jump tuning
    CharacterTuning.JumpTuning JumpTuning {
        get => c.Tuning.Jumps[c.State.JumpTuningIndex];
    }

    /// if this is the character's first (grounded) jump
    bool IsFirstJump {
        get => c.State.JumpTuningIndex == 0 && c.State.JumpTuningJumpIndex == 0;
    }

    /// if the character can jump within the last n frames
    bool ShouldStartJump(int buffer = 0) {
        if (!HasJump()) {
            return false;
        }

        var frame = c.Input.GetJumpDown(buffer);
        if (frame == -1) {
            return false;
        }

        for (var i = 0; i < frame; i++) {
            if (c.State[i].Events.Contains(CharacterEvent.Jump)) {
                return false;
            }
        }

        return true;
    }

    /// if the character has a jump available to execute
    bool HasJump() {
        // if the character can't ever jump
        if (c.Tuning.Jumps.Length == 0) {
            return false;
        }

        // start jump if jump is pressed before coyote frames expire
        // a few frames in jump squat before falling again
        // NOTE: we could sorta fix this by skipping jump squat, requiring the whole
        // jump finish here, and transitioning directly to jump

        // if it's your first jump, account for coyote time
        if (IsFirstJump && c.State.CoyoteFrames >= 0) {
            return true;
        }

        if (c.State.CooldownFrames > 0) {
            return false;
        }

        // zero count means infinite jumps
        if (JumpTuning.Count == 0) {
            return true;
        }

        // start an air jump if available
        // if there's still jumps available in the current jump definition
        if (c.State.JumpTuningJumpIndex < JumpTuning.Count) {
            return true;
        }

        return false;
    }

    /// if the character is on something ground like
    bool IsOnGround() {
        var ground = c.State.Curr.MainSurface;
        if (ground.IsNone) {
            return false;
        }

        return ground.Angle <= c.Tuning.Jump_GroundAngle;
    }
}

}