using System;
using Soil;
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
    protected override Phase<CharacterContainer> InitInitialPhase() {
        return NotJumping;
    }

    protected override SystemState State {
        get => c.State.Next.JumpState;
        set => c.State.Next.JumpState = value;
    }

    // -- lifecycle --
    public override void Init(CharacterContainer c) {
        base.Init(c);
        ResetJumps(c);
    }

    public override void Update(float delta) {
        // advance the cooldown timer
        Cooldown(delta, c);

        // jumping updates state that is used below
        base.Update(delta);

        // aggregate gravity
        var gravity = 0f;

        // add base gravity
        if (c.State.Next.Velocity.y > 0f) {
            gravity += c.Tuning.Gravity_Jump;
        } else {
            gravity += c.Tuning.Gravity;
        }

        // calculate and add lift
        var lift = 0f;
        var jumpElapsed = c.State.Next.Jump_Elapsed;
        var jumpReleasedAt = c.State.Next.Jump_ReleasedAt;

        // add jump lift when not holding jump (adsr)
        if (!c.Inputs.IsJumpPressed) {
            // if we released the jump, track release time
            if (jumpReleasedAt == AdsrCurve.NotReleased) {
                jumpReleasedAt = jumpElapsed;
            }

            // add the jump adsr gravity
            var jumpTuning = c.Tuning.JumpById(c.State.Next.ActiveJump);
            lift = jumpTuning.Lift.Evaluate(jumpElapsed, jumpReleasedAt);
        }
        // add charge lift when holding jump
        else {
            // TODO: should crouch gravity on surface go here?
            // should this be curved by surface angle
            var jumpTuning = c.Tuning.JumpById(c.State.Next.NextJump);
            lift += jumpTuning.Charge_Lift;
        }

        // scale lift based on surface, if any
        var surface = c.State.Next.MainSurface;
        var surfaceScale = 1f;
        if (surface.IsSome) {
            surfaceScale = c.Tuning.Surface_VerticalGrip_Scale.Evaluate(surface.Angle);
        }

        gravity += lift * surfaceScale;

        // track jump time
        jumpElapsed += delta;

        // apply gravity, update state
        // TODO: curve gravity on jumpsquat elapsed?
        c.State.Next.Force += gravity * Vector3.up;
        c.State.Next.Jump_Elapsed = jumpElapsed;
        c.State.Next.Jump_ReleasedAt = jumpReleasedAt;
    }

    // -- NotJumping --
    static readonly Phase<CharacterContainer> NotJumping = new("NotJumping",
        update: NotJumping_Update
    );

    static void NotJumping_Update(float delta, System<CharacterContainer> s, CharacterContainer c) {
        // reset jump surface whenever grounded
        if (IsOnSurface(c)) {
            ResetJumpSurface(c);
        }
        // but if not, subtract delta
        else {
            c.State.Next.CoyoteTime -= delta;
        }

        // fall once coyote time expires
        if (c.State.Next.CoyoteTime <= 0f) {
            s.ChangeTo(Falling);
            return;
        }

        // if you jump
        if (ShouldStartJump(c, c.Tuning.Jump_BufferDuration)) {
            s.ChangeTo(JumpSquat);
            return;
        }
    }

    // -- Landing --
    static readonly Phase<CharacterContainer> Landing = new("Landing",
        enter: Landing_Enter,
        update: Landing_Update
    );

    static void Landing_Enter(System<CharacterContainer> _, CharacterContainer c) {
        ResetJumps(c);
        ResetJumpSurface(c);

        c.Events.Schedule(CharacterEvent.Land);
    }

    static void Landing_Update(float delta, System<CharacterContainer> s, CharacterContainer c) {
        // reset jump surface whenever grounded
        if (IsOnSurface(c)) {
            ResetJumpSurface(c);
        }
        // but if not, subtract a delta
        else {
            c.State.Next.CoyoteTime -= delta;
        }

        // fall once coyote time expires
        if (c.State.Next.CoyoteTime <= 0f) {
            s.ChangeTo(Falling);
            return;
        }

        // if you jump
        if (ShouldStartJump(c, c.Tuning.Jump_BufferDuration)) {
            s.ChangeTo(JumpSquat);
            return;
        }

        // once landing completes
        if (s.PhaseElapsed > c.Tuning.LandingDuration) {
            s.ChangeTo(NotJumping);
            return;
        }
    }

    // -- JumpSquat --
    static readonly Phase<CharacterContainer> JumpSquat = new("JumpSquat",
        enter: JumpSquat_Enter,
        update: JumpSquat_Update,
        exit: JumpSquat_Exit
    );

    static void JumpSquat_Enter(System<CharacterContainer> _, CharacterContainer c) {
        c.State.Next.IsInJumpSquat = true;
    }

    static void JumpSquat_Update(float delta, System<CharacterContainer> s, CharacterContainer c) {
        var jumpTuning = c.Tuning.NextJump(c.State);

        // jump if jump was released or jump squat ended
        var shouldJump = (
            // if jump was released after the minimum
            // TODO: should we buffer jump released?
            (!c.Inputs.IsJumpPressed && s.PhaseElapsed >= jumpTuning.Charge_Duration.Min) ||
            // or if the jump squat finished
            // TODO: custom editor to hide max if unused
            (jumpTuning.Charge_ShouldAutoJump && s.PhaseElapsed >= jumpTuning.Charge_Duration.Max)
        );

        if (shouldJump) {
            Jump(s.PhaseElapsed, c);
            s.ChangeTo(Falling);
            return;
        }

        // reset back to initial jump & jump surface whenever grounded
        // TODO: support a mario-like triple jump?
        var isOnSurface = IsOnSurface(c);
        if (isOnSurface) {
            ResetJumps(c);
            ResetJumpSurface(c);
            jumpTuning = c.Tuning.NextJump(c.State);
        }

        // if this is the first jump, you might be in coyote time
        if (IsFirstJump(c)) {
            // if airborne, reduce coyote time
            if (!isOnSurface) {
                c.State.Next.CoyoteTime -= delta;
            }

            // when coyote time expires, consume this jump
            if (c.State.Next.CoyoteTime <= 0) {
                // BUG: advancing jumps twice?
                AdvanceJumps(c);

                // if we're out of jumps, fall instead
                if (!HasJump(c)) {
                    s.ChangeTo(Falling);
                    return;
                }
            }
        }
    }

    static void JumpSquat_Exit(System<CharacterContainer> _, CharacterContainer c) {
        // NOTE: do we force the jump here?
        c.State.Next.IsInJumpSquat = false;
    }

    // -- Falling --
    static readonly Phase<CharacterContainer> Falling = new("Falling",
        enter: Falling_Enter,
        update: Falling_Update
    );

    static void Falling_Enter(System<CharacterContainer> _, CharacterContainer c) {
        AdvanceJumps(c);
    }

    static void Falling_Update(float delta, System<CharacterContainer> s, CharacterContainer c) {
        // update timers
        c.State.Next.CoyoteTime -= delta;

        // start jump on a new press
        if (ShouldStartJump(c)) {
            s.ChangeTo(JumpSquat);
            return;
        }

        // transition out of jump
        if (IsOnSurface(c)) {
            s.ChangeTo(Landing);
            return;
        }
    }

    // -- commands --
    /// add the jump impulse for this jump index
    static void Jump(float elapsed, CharacterContainer c) {
        var jumpId = c.State.Next.NextJump;
        var jumpTuning = c.Tuning.JumpById(jumpId);

        // accumulate jump delta dv
        var v0 = c.State.Curr.Velocity;
        var dv = Vector3.zero;

        // get curved percent complete through jump squat
        var power = jumpTuning.Power(elapsed);

        // the cached surface we're jumping off of
        var surface = c.State.Next.JumpSurface;

        // scale jumps based on surface, if any
        var surfaceScale = 1f;
        if (surface.IsSome) {
            surfaceScale = c.Tuning.Jump_SurfaceAngleScale.Evaluate(surface.Angle);
        }

        // cancel vertical momentum if falling. according to tuning if going up (we don't want to lose
        // upwards speed in general, but not jumping if too fast is too weird)
        // TODO: add downwards momentum loss so that air jumps can be very bad
        var verticalLoss = v0.y > 0f ? jumpTuning.Upwards_MomentumLoss : 1f;
        dv -= surfaceScale * v0.y * verticalLoss * Vector3.up;

        // cancel horizontal momentum
        var horizontalLoss = jumpTuning.Horizontal_MomentumLoss;
        dv -= surfaceScale * horizontalLoss * c.State.Curr.PlanarVelocity;

        // add directional jump velocity
        // TODO: change Vector3.up to JumpTuning.Direction
        var jumpSpeed = jumpTuning.Vertical_Speed.Evaluate(power);
        dv += jumpSpeed * surfaceScale * Vector3.up;

        // add surface normal jump velocity
        if (surface.IsSome) {
            var normalSpeed = c.Tuning.Jump_Normal_Speed.Evaluate(power);
            var normalScale = c.Tuning.Jump_Normal_SurfaceAngleScale.Evaluate(surface.Angle);
            dv += normalSpeed * normalScale * surface.Normal;
        }

        // update state
        c.State.Next.Inertia = 0f;
        c.State.Next.Velocity += dv;
        c.State.Next.CoyoteTime = 0f;

        c.State.Next.ActiveJump = jumpId;
        c.State.Next.Jump_Elapsed = 0f;
        c.State.Next.Jump_ReleasedAt = AdsrCurve.NotReleased;

        c.State.Next.Jump_CooldownElapsed = 0f;
        c.State.Next.Jump_CooldownDuration = jumpTuning.CooldownDuration.Evaluate(power);

        c.Events.Schedule(CharacterEvent.Jump);
    }

    /// add jump cooldown, if any
    static void Cooldown(float delta, CharacterContainer c) {
        // update jump cooldown
        var nextElapsed = c.State.Curr.Jump_CooldownElapsed;
        var nextDuration = c.State.Curr.Jump_CooldownDuration;

        // if we are in cooldown
        if (nextElapsed < nextDuration) {
            nextElapsed += delta;

            // if we are overflowing, this frame ends cooldown, and
            // we want to check for a buffered jump
            if (nextElapsed >= nextDuration) {
                // TODO: a cooldown elapsed event?
                nextElapsed = nextDuration;
            }
        }
        // otherwise, there's no cooldown, so reset
        else {
            nextElapsed = 0f;
            nextDuration = 0f;
        }

        c.State.Next.Jump_CooldownElapsed = nextElapsed;
        c.State.Next.Jump_CooldownDuration = nextDuration;
    }

    /// track jump and switch to the correct jump if necessary
    static void AdvanceJumps(CharacterContainer c) {
        var nextJump = c.State.Curr.NextJump;

        // cache the active jump
        var activeJump = nextJump;

        // advance next jump
        nextJump.Count += 1;

        var jumpTuning = c.Tuning.JumpById(nextJump);
        if (jumpTuning.Count == 0) {
            return;
        }

        var shouldAdvanceJump = (
            nextJump.Count >= jumpTuning.Count &&
            nextJump.Index < c.Tuning.Jumps.Length - 1
        );

        if (shouldAdvanceJump) {
            nextJump.Count = 0;
            nextJump.Index += 1;
        }

        c.State.Next.ActiveJump = activeJump;
        c.State.Next.NextJump = nextJump;
    }

    /// reset the jump count to its initial state
    static void ResetJumps(CharacterContainer c) {
        var nextJump = c.State.Curr.NextJump;
        nextJump.Index = 0;
        nextJump.Count = 0;
        c.State.Next.NextJump = nextJump;
    }

    /// reset the next surface to jump from
    static void ResetJumpSurface(CharacterContainer c) {
        c.State.Next.CoyoteTime = c.Tuning.CoyoteDuration;
        c.State.Next.JumpSurface = c.State.Curr.PerceivedSurface;
    }

    // -- queries --
    /// if this is the character's first (grounded) jump
    static bool IsFirstJump(CharacterContainer c) {
        return c.State.Next.NextJump.Index == 0 && c.State.Next.NextJump.Count == 0;
    }

    /// if the character should jump within the last n frames
    static bool ShouldStartJump(CharacterContainer c, float buffer = 0) {
        return HasJump(c) && c.Inputs.IsJumpPressedInBuffer(Mathf.Max(buffer, c.State.Next.Jump_CooldownElapsed));
    }

    /// if the character has a jump available to execute
    static bool HasJump(CharacterContainer c) {
        // if the character can't ever jump
        if (c.Tuning.Jumps.Length == 0) {
            return false;
        }

        // can't jump while in cooldown
        if (c.State.Next.Jump_CooldownElapsed < c.State.Next.Jump_CooldownDuration) {
            return false;
        }

        // start jump if jump is pressed before coyote frames expire
        // a few frames in jump squat before falling again
        // NOTE: we could sorta fix this by skipping jump squat, requiring the whole
        // jump finish here, and transitioning directly to jump

        // if it's your first jump, account for coyote time
        if (IsFirstJump(c) && c.State.Next.CoyoteTime >= 0f) {
            return true;
        }

        // zero count means infinite jumps
        var jumpTuning = c.Tuning.NextJump(c.State);
        if (jumpTuning.Count == 0) {
            return true;
        }

        // start an air jump if available, if there's still jumps available in the current jump definition
        if (c.State.Next.NextJump.Count < jumpTuning.Count) {
            return true;
        }

        return false;
    }

    /// if the character is on something ground like
    static bool IsOnSurface(CharacterContainer c) {
        return c.State.Curr.MainSurface.IsSome;
    }
}

}