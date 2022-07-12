using System;
using UnityEngine;

namespace ThirdPerson {

/// the character's authoritative state
public sealed partial class CharacterState {
    // -- props --
    /// the queue of frames
    Queue<Frame> m_Frames;

    // -- lifetime --
    public CharacterState(Vector3 position, Vector3 forward) {
        m_Frames = new Queue<Frame>(5);
        var frame = new Frame(position, forward);
        Fill(frame);
    }

    // -- props/hot --
    /// the current frame
    public Frame CurrentFrame {
        get => m_Frames[0];
        set => m_Frames[0] = value;
    }

    // -- commands --
    /// snapshot the current state
    public void Snapshot() {
        m_Frames.Add(new Frame(m_Frames[0]));
    }

    /// fill the queue with the frame
    public void Fill(CharacterState.Frame frame) {
        m_Frames.Fill(frame);
    }

    /// sets the forward direction on the xz plane
    public void SetProjectedForward(Vector3 dir) {
        var projected = Vector3.ProjectOnPlane(dir, Up);

        // if zero, use the original direction
        if (projected.sqrMagnitude > 0.0f) {
            Forward = projected.normalized;
        }
    }

    // -- queries --
    /// if the state has no frames
    public bool IsEmpty {
        get => m_Frames.IsEmpty;
    }

    /// if currently idle
    public bool IsIdle {
        get => m_Frames[0].IdleTime > 0.0f;
    }

    /// get the nth most recent frame
    public Frame GetFrame(uint i) {
        return m_Frames[i];
    }

    /// the character's up vector (hardcoded to Vector3.up)
    public Vector3 Up {
        get => m_Frames[0].Up;
    }

    /// the character's look rotation (facing & tilt)
    public Quaternion LookRotation {
        get => m_Frames[0].LookRotation;
    }

    /// the velocity on the xz-plane
    /// TODO: maybe this should be grounded velocity, since the places its been used are ground related
    public Vector3 PlanarVelocity {
        get => m_Frames[0].Velocity.XNZ();
    }

    /// the character's current acceleration
    public Vector3 Acceleration {
        get {
            var v0 = m_Frames[1].Velocity;
            var v1 = m_Frames[0].Velocity;
            return (v1 - v0) / Time.fixedDeltaTime;
        }
    }

    // -- types --
    /// a single frame of character state
    [Serializable]
    public sealed partial class Frame: IEquatable<Frame> {
        // -- props --
        /// the world position
        public Vector3 Position;

        /// the character's velocity
        public Vector3 Velocity = Vector3.zero;

        /// the current facing direction
        public Vector3 Forward;

        /// if the character is grounded
        public bool IsGrounded = false;

        /// if the character is in jump squat
        public bool IsInJumpSquat = false;

        /// if the character is in its first jump frame
        public bool IsInJumpStart = false;

        /// if the characer is on the wall
        public bool IsOnWall = false;

        /// how much tilted the character is
        public Quaternion Tilt = Quaternion.identity;

        /// the collision information for the previous frame
        public CharacterCollision Collision;

        /// the current frame in the jump squat
        public int JumpSquatFrame = -1;

        /// the current time the character hasn't moved
        public float IdleTime = 0;

        ///  the current frame in pivot animation
        public int PivotFrame = -1;

        ///  the direction of the current pivot
        public Vector3 PivotDirection = Vector3.zero;

        /// the number of jumps executed since last grounded
        public uint Jumps = 0;

        // -- lifetime --
        /// create an empty frame
        public Frame() {
        }

        /// create an initial frame
        public Frame(Vector3 position, Vector3 forward) {
            Position = position;
            Forward = forward;
        }

        // -- queries --
        /// the character's up vector (hardcoded to Vector3.up)
        public Vector3 Up {
            get => Vector3.up;
        }

        /// the character's look rotation (facing & tilt)
        public Quaternion LookRotation {
            get {
                return Tilt * Quaternion.LookRotation(Forward, Up);
            }
        }
    }
}
}