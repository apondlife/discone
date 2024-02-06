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
        if (IsOnSurface()) {
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
        if (IsOnSurface()) {
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
        AddJumpGravity();

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

        // reset back to initial jump & jump surface whenever grounded
        // TODO: support a mario-like triple jump?
        var isOnSurface = IsOnSurface();
        if (isOnSurface) {
            ResetJumps();
            ResetJumpSurface();
        }

        // if this is the first jump, you might be in coyote time
        if (IsFirstJump) {
            // if airborne, reduce coyote time
            if (!isOnSurface) {
                c.State.CoyoteFrames -= 1;
            }

            // when coyote time expires, consume this jump
            if (c.State.CoyoteFrames <= 0) {
                IncrementJumps();

                // if we're out of jumps, fall instead
                if (!HasJump()) {
                    ChangeTo(Falling);
                    return;
                }
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
        AddJumpGravity();

        // count coyote frames
        c.State.CoyoteFrames -= 1;
        c.State.CooldownFrames -= 1;

        // start jump on a new press
        if (ShouldStartJump()) {
            ChangeTo(JumpSquat);
            return;
        }

        // transition out of jump
        if (IsOnSurface()) {
            ChangeTo(Landing);
            return;
        }
    }

    // -- commands --
    /// reset the next surface to jump from
    void ResetJumpSurface() {
        c.State.Next.CoyoteFrames = c.Tuning.MaxCoyoteFrames;
        c.State.Next.JumpSurface = c.State.Next.MainSurface;
    }

    /// add jump anti-gravity when holding the button
    void AddJumpGravity() {
        if (!c.Input.IsJumpPressed || c.State.Curr.MainSurface.IsSome) {
            return;
        }

        var acceleration = c.State.Curr.Velocity.y > 0.0f
            ? c.Tuning.JumpGravity // TODO: what if you are going up but not from a jump?
            : c.Tuning.FallGravity;

        acceleration -= c.Tuning.Gravity;

        c.State.Next.Force += acceleration * Vector3.up;
    }

    /// add the jump impulse for this jump index
    void Jump() {
        // accumulate jump delta dv
        var v0 = c.State.Curr.Velocity;
        var dv = Vector3.zero;

        // get curved percent complete through jump squat
        var pct = Mathf.InverseLerp(
            JumpTuning.MinJumpSquatFrames,
            JumpTuning.MaxJumpSquatFrames,
            c.State.Next.JumpSquatFrame
        );

        // AAA: should this work like this for air jumps?
        var surfaceScale = c.Tuning.Jump_SurfaceAngleScale.Evaluate(c.State.Next.JumpSurface.Angle);

        // cancel vertical momentum if falling.
        // according to tuning if going up
        // (we don't want to lose upwards speed in general, but not jumping if too fast is too weird)
        var verticalLoss = v0.y > 0 ? JumpTuning.Upwards_MomentumLoss : 1;
        dv -= surfaceScale * v0.y * verticalLoss * Vector3.up;

        // cancel horizontal momentum
        var horizontalLoss = JumpTuning.Horizontal_MomentumLoss;
        dv -= surfaceScale * horizontalLoss * c.State.Curr.PlanarVelocity;

        // add directional jump velocity
        // TODO: change Vector3.up to JumpTuning.Direction
        var jumpSpeed = Mathf.Lerp(
            JumpTuning.Vertical_MinSpeed,
            JumpTuning.Vertical_MaxSpeed,
            JumpTuning.Vertical_SpeedCurve.Evaluate(pct)
        );

        dv += jumpSpeed * surfaceScale * Vector3.up;

        // add surface normal jump velocity
        if (c.State.Next.PerceivedSurface.IsSome) {
            var normalSpeed = c.Tuning.Jump_Normal_Speed.Evaluate(pct);
            var normalSurface = c.State.Next.PerceivedSurface;
            var normalScale = c.Tuning.Jump_Normal_SurfaceAngleScale.Evaluate(normalSurface.Angle);
            dv += normalSpeed * normalScale * normalSurface.Normal;

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
        c.State.Next.JumpTuningJumpIndex += 1;

        if (JumpTuning.Count == 0) {
            return;
        }

        var shouldAdvanceJump = (
            c.State.Next.JumpTuningJumpIndex >= JumpTuning.Count &&
            c.State.Next.JumpTuningIndex < c.Tuning.Jumps.Length - 1
        );

        if (shouldAdvanceJump) {
            c.State.Next.JumpTuningJumpIndex = 0;
            c.State.Next.JumpTuningIndex += 1;
        }
    }

    /// reset the jump count to its initial state
    void ResetJumps() {
        c.State.Next.Jumps = 0;
        c.State.Next.JumpTuningJumpIndex = 0;
        c.State.Next.JumpTuningIndex = 0;
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

    /// if the character should jump within the last n frames
    bool ShouldStartJump(int buffer = 1) {
        return HasJump() && HasJumpInput(buffer);
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
        if (IsFirstJump && c.State.Next.CoyoteFrames >= 0) {
            return true;
        }

        if (c.State.Next.CooldownFrames > 0) {
            return false;
        }

        // zero count means infinite jumps
        if (JumpTuning.Count == 0) {
            return true;
        }

        // start an air jump if available
        // if there's still jumps available in the current jump definition
        if (c.State.Next.JumpTuningJumpIndex < JumpTuning.Count) {
            return true;
        }

        return false;
    }

    /// if the player had a new jump input within the last n frames
    bool HasJumpInput(int buffer = 1) {
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

    /// if the character is on something ground like
    bool IsOnSurface() {
        return c.State.Curr.MainSurface.IsSome;
    }
}

}