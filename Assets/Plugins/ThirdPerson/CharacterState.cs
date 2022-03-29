using System;
using UnityEngine;

namespace ThirdPerson {

/// the character's authoritative state
public sealed class CharacterState {
    // -- fields --
    [Header("fields")]
    [Tooltip("the world position")]
    public Vector3 Position;

    [Tooltip("the velocity on the xz-plane")]
    public Vector3 PlanarVelocity;

    [Tooltip("the speed on the y-axis")]
    public float VerticalSpeed = 0;

    [Tooltip("the current facing direction")]
    public Vector3 FacingDirection;

    [Tooltip("if the character is grounded")]
    public bool IsGrounded = false;

    [Tooltip("if the character is in jump squat")]
    public bool IsInJumpSquat = false;

    [Tooltip("if the character is in its first jump frame")]
    public bool IsInJumpStart = false;

    [Tooltip("if the characer is on the wall")]
    public bool IsOnWall;

    [Tooltip("how much tilted the character is")]
    public Quaternion Tilt;

    [Tooltip("the most recent collision")]
    public CharacterCollision? Collision;

    [Tooltip("the current jump squat frame")]
    public int JumpSquatFrame;

    // -- props --
    Queue<Frame> m_Frames = new Queue<Frame>(5);

    // -- commands --
    /// snapshot the current state
    public void Snapshot() {
        m_Frames.Add(new Frame(this));
    }

    /// sets the velocity
    public void SetVelocity(Vector3 v) {
        VerticalSpeed = v.y;
        PlanarVelocity = v.XNZ();
    }

    /// sets the planar direction on the xz plane
    public void SetProjectedPlanarVelocity(Vector3 dir) {
        var projected = Vector3.ProjectOnPlane(dir, Up);

        // if zero, use the original direction
        if (projected.sqrMagnitude > 0.0f) {
            PlanarVelocity = projected;
        }
    }

    /// sets the facing direction on the xz plane
    public void SetProjectedFacingDirection(Vector3 dir) {
        var projected = Vector3.ProjectOnPlane(dir, Up);

        // if zero, use the original direction
        if (projected.sqrMagnitude > 0.0f) {
            FacingDirection = projected.normalized;
        }
    }

    /// sets the last state frame
    public void SetLastFrame(Frame frame) {
        // m_Frames[0] = frame;
    }

    // -- queries --
    /// the character's up vector (hardcoded to Vector3.up)
    public Vector3 Up {
        get => Vector3.up;
    }

    /// the character's velocity in 3d-space
    public Vector3 Velocity {
        get => GetVelocity(PlanarVelocity, VerticalSpeed);
    }

    /// the character's current acceleration
    public Vector3 Acceleration {
        get {
            var f = m_Frames[0];
            var v0 = GetVelocity(f.PlanarVelocity, f.VerticalSpeed);
            var v1 = Velocity;
            return (v1 - v0) / Time.fixedDeltaTime;
        }
    }

    /// the planar velocity last frame
    public Vector3 PrevPlanarVelocity {
        get => m_Frames[0].PlanarVelocity;
    }

    /// the characters look rotation (facing & tilt)
    public Quaternion LookRotation {
        get {
            var look = FacingDirection;

            // if airborne, look in direction of velocity
            // TODO: we don't want to calculate a look direction from a zero velocity,
            // but is using FacingDirection in this situation correct?
            if (!IsGrounded && PlanarVelocity != Vector3.zero) {
                look = PlanarVelocity.normalized;
            }

            return Tilt * Quaternion.LookRotation(look, Up);
        }
    }

    Vector3 GetVelocity(Vector3 planarVelocity, float verticalSpeed) {
        return planarVelocity + verticalSpeed * Up;
    }

    // -- types --
    [Serializable]
    public readonly struct Frame {
        // -- props --
        // the world position
        public readonly Vector3 Position;

        // the velocity on the xz-plane
        public readonly Vector3 PlanarVelocity;

        // the speed on the y-axis
        public readonly float VerticalSpeed;

        // how much velocity changed since last update
        public readonly Vector3 Acceleration;

        // the current facing direction
        public readonly Vector3 FacingDirection;

        // if the character is grounded
        public readonly bool IsGrounded;

        // if the character is in jump squat
        public readonly bool IsInJumpSquat;

        // if the character is in its first jump frame
        public readonly bool IsInJumpStart;

        // if the characer is on the wall
        public readonly bool IsOnWall;

        // how much tilted the character is
        public readonly Quaternion Tilt;

        // -- lifetime --
        /// capture a frame from state
        public Frame(CharacterState s) {
            Position = s.Position;
            PlanarVelocity = s.PlanarVelocity;
            VerticalSpeed = s.VerticalSpeed;
            Acceleration = s.Acceleration;
            FacingDirection = s.FacingDirection;
            IsGrounded = s.IsGrounded;
            IsInJumpSquat = s.IsInJumpSquat;
            IsInJumpStart = s.IsInJumpStart;
            IsOnWall = s.IsOnWall;
            Tilt = s.Tilt;
        }
    }
}

}