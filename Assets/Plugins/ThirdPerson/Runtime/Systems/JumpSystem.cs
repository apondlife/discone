﻿using System;
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

        // always add gravity
        c.State.Next.Force += c.Tuning.Gravity * Vector3.up;

        base.Update(delta);
    }

    // -- NotJumping --
    static readonly Phase<CharacterContainer> NotJumping = new("NotJumping",
        enter: (_, c) => {
            c.State.Next.IsLanding = false;
        },
        update: (delta, s, c) => {
            // reset jump surface whenever grounded
            if (IsOnSurface(c)) {
                ResetJumpSurface(c);
            }
            // but if not, subtract delta
            else {
                c.State.CoyoteTime -= delta;
            }

            // fall once coyote time expires
            if (c.State.CoyoteTime <= 0f) {
                s.ChangeTo(Falling);
                return;
            }

            // if you jump
            if (ShouldStartJump(c, c.Tuning.Jump_BufferDuration)) {
                s.ChangeTo(JumpSquat);
                return;
            }
        }
    );

    // -- Landing --
    static readonly Phase<CharacterContainer> Landing = new("Landing",
        enter: (_, c) => {
            ResetJumps(c);
            c.State.Next.IsLanding = true;
            c.Events.Schedule(CharacterEvent.Land);
        },
        update: (delta, s, c) => {
            // reset jump surface whenever grounded
            if (IsOnSurface(c)) {
                ResetJumpSurface(c);
            }
            // but if not, subtract a delta
            else {
                c.State.CoyoteTime -= delta;
            }

            // fall once coyote time expires
            if (c.State.CoyoteTime <= 0f) {
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
    );

    // -- JumpSquat --
    static readonly Phase<CharacterContainer> JumpSquat = new("JumpSquat",
        enter: (_, c) => {
            c.State.Next.IsInJumpSquat = true;
        },
        update: (delta, s, c) => {
            AddJumpGravity(c);

            // jump if jump was released or jump squat ended
            var shouldJump = (
                // if the jump squat finished
                s.PhaseElapsed >= GetJumpTuning(c).JumpSquatDuration.Max ||
                // or jump was released after the minimum
                // TODO: should we buffer jump released?
                (!c.Inputs.IsJumpPressed && s.PhaseElapsed >= GetJumpTuning(c).JumpSquatDuration.Min)
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
            }

            // if this is the first jump, you might be in coyote time
            if (IsFirstJump(c)) {
                // if airborne, reduce coyote time
                if (!isOnSurface) {
                    c.State.CoyoteTime -= delta;
                }

                // when coyote time expires, consume this jump
                if (c.State.CoyoteTime <= 0) {
                    AdvanceJumps(c);

                    // if we're out of jumps, fall instead
                    if (!HasJump(c)) {
                        s.ChangeTo(Falling);
                        return;
                    }
                }
            }
        },
        exit: (_, c) => {
            // NOTE: do we force the jump here?
            c.State.Next.IsInJumpSquat = false;
        }
    );

    // -- Falling --
    static readonly Phase<CharacterContainer> Falling = new("Falling",
        enter: (_, c) => {
            AdvanceJumps(c);
            c.State.Next.IsLanding = false;
        },
        update: (delta, s, c) => {
            AddJumpGravity(c);

            // update timers
            c.State.CoyoteTime -= delta;

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
    );

    // -- commands --
    /// reset the next surface to jump from
    static void ResetJumpSurface(CharacterContainer c) {
        c.State.Next.CoyoteTime = c.Tuning.CoyoteDuration;
        c.State.Next.JumpSurface = c.State.Curr.PerceivedSurface;
    }

    /// add jump anti-gravity when holding the button
    static void AddJumpGravity(CharacterContainer c) {
        if (!c.Inputs.IsJumpPressed || c.State.Curr.MainSurface.IsSome) {
            return;
        }

        var acceleration = c.State.Curr.Velocity.y > 0.0f
            ? c.Tuning.JumpGravity // TODO: what if you are going up but not from a jump?
            : c.Tuning.FallGravity;

        acceleration -= c.Tuning.Gravity;

        c.State.Next.Force += acceleration * Vector3.up;
    }

    /// add the jump impulse for this jump index
    static void Jump(float elapsed, CharacterContainer c) {
        // accumulate jump delta dv
        var v0 = c.State.Curr.Velocity;
        var dv = Vector3.zero;

        // get curved percent complete through jump squat
        var pct = GetJumpTuning(c).JumpSquatDuration.InverseLerp(elapsed);

        // the cached surface we're jumping off of
        var surface = c.State.Next.JumpSurface;

        // scale jumps based on surface, if any
        var surfaceScale = 1f;
        if (surface.IsSome) {
            surfaceScale = c.Tuning.Jump_SurfaceAngleScale.Evaluate(c.State.Next.JumpSurface.Angle);
        }

        // cancel vertical momentum if falling. according to tuning if going up (we don't want to lose
        // upwards speed in general, but not jumping if too fast is too weird)
        // TODO: add downwards momentum loss so that air jumps can be very bad
        var verticalLoss = v0.y > 0f ? GetJumpTuning(c).Upwards_MomentumLoss : 1f;
        dv -= surfaceScale * v0.y * verticalLoss * Vector3.up;

        // cancel horizontal momentum
        var horizontalLoss = GetJumpTuning(c).Horizontal_MomentumLoss;
        dv -= surfaceScale * horizontalLoss * c.State.Curr.PlanarVelocity;

        // add directional jump velocity
        // TODO: change Vector3.up to JumpTuning.Direction
        var jumpSpeed = GetJumpTuning(c).Vertical_Speed.Evaluate(pct);
        dv += jumpSpeed * surfaceScale * Vector3.up;

        // add surface normal jump velocity
        if (surface.IsSome) {
            var normalSpeed = c.Tuning.Jump_Normal_Speed.Evaluate(pct);
            var normalScale = c.Tuning.Jump_Normal_SurfaceAngleScale.Evaluate(surface.Angle);
            dv += normalSpeed * normalScale * surface.Normal;
        }

        // update state
        c.State.Next.Inertia = 0f;
        c.State.Next.Velocity += dv;
        c.State.Next.CoyoteTime = 0f;
        c.State.Next.Jump_CooldownElapsed = 0f;
        c.State.Next.Jump_CooldownDuration = GetJumpTuning(c).CooldownDuration.Evaluate(pct);

        c.Events.Schedule(CharacterEvent.Jump);
    }

    /// add jump cooldown, if any
    static void Cooldown(float delta, CharacterContainer c) {
        // update jump cooldown
        var nextElapsed = c.State.Curr.Jump_CooldownElapsed;;
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
        c.State.Next.Jumps += 1;
        c.State.Next.JumpTuningJumpIndex += 1;

        if (GetJumpTuning(c).Count == 0) {
            return;
        }

        var shouldAdvanceJump = (
            c.State.Next.JumpTuningJumpIndex >= GetJumpTuning(c).Count &&
            c.State.Next.JumpTuningIndex < c.Tuning.Jumps.Length - 1
        );

        if (shouldAdvanceJump) {
            c.State.Next.JumpTuningJumpIndex = 0;
            c.State.Next.JumpTuningIndex += 1;
        }
    }

    /// reset the jump count to its initial state
    static void ResetJumps(CharacterContainer c) {
        c.State.Next.Jumps = 0;
        c.State.Next.JumpTuningJumpIndex = 0;
        c.State.Next.JumpTuningIndex = 0;
    }

    // -- queries --
    /// the current jump tuning
    static CharacterTuning.JumpTuning GetJumpTuning(CharacterContainer c) {
        return c.Tuning.Jumps[c.State.JumpTuningIndex];
    }

    /// if this is the character's first (grounded) jump
    static bool IsFirstJump(CharacterContainer c) {
        return c.State.JumpTuningIndex == 0 && c.State.JumpTuningJumpIndex == 0;
    }

    /// if the character should jump within the last n frames
    static bool ShouldStartJump(CharacterContainer c, float buffer = 0) {
        return HasJump(c) && HasJumpInput(Mathf.Max(buffer, c.State.Next.Jump_CooldownElapsed), c);
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
        if (GetJumpTuning(c).Count == 0) {
            return true;
        }

        // start an air jump if available
        // if there's still jumps available in the current jump definition
        if (c.State.Next.JumpTuningJumpIndex < GetJumpTuning(c).Count) {
            return true;
        }

        return false;
    }

    /// if the player had a new jump input within the last n frames
    static bool HasJumpInput(float buffer, CharacterContainer c) {
        return c.Inputs.IsJumpDown(buffer);
    }

    /// if the character is on something ground like
    static bool IsOnSurface(CharacterContainer c) {
        return c.State.Curr.MainSurface.IsSome;
    }
}

}