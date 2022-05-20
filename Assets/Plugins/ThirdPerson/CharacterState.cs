using System;
using UnityEngine;

namespace ThirdPerson {

/// the character's authoritative state
public sealed class CharacterState {
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

    /// the position
    public Vector3 Position {
        get => m_Frames[0].Position;
        set => m_Frames[0].Position = value;
    }

    /// the character's velocity in 3d-space
    public Vector3 Velocity {
        get => m_Frames[0].Velocity;
        set => m_Frames[0].Velocity = value;
    }

    /// the current facing direction
    public Vector3 FacingDirection {
        get => m_Frames[0].FacingDirection;
        set => m_Frames[0].FacingDirection = value;
    }

    public int PivotFrame {
        get => m_Frames[0].PivotFrame;
        set => m_Frames[0].PivotFrame = value;
    }

    public Vector3 PivotDirection {
        get => m_Frames[0].PivotDirection;
        set => m_Frames[0].PivotDirection = value;
    }

    /// if the character is grounded
    public bool IsGrounded {
        get => m_Frames[0].IsGrounded;
        set => m_Frames[0].IsGrounded = value;
    }

    /// if the character is in jump squat
    public bool IsInJumpSquat {
        get => m_Frames[0].IsInJumpSquat;
        set => m_Frames[0].IsInJumpSquat = value;
    }

    /// if the character is in its first jump frame
    public bool IsInJumpStart {
        get => m_Frames[0].IsInJumpStart;
        set => m_Frames[0].IsInJumpStart = value;
    }

    /// if the characer is on the wall
    public bool IsOnWall {
        get => m_Frames[0].IsOnWall;
        set => m_Frames[0].IsOnWall = value;
    }

    /// how much tilted the character is
    public Quaternion Tilt {
        get => m_Frames[0].Tilt;
        set => m_Frames[0].Tilt = value;
    }

    /// the most recent collision
    public CharacterCollision Collision {
        get => m_Frames[0].Collision;
        set => m_Frames[0].Collision = value;
    }

    /// the current jump squat frame
    public int JumpSquatFrame {
        get => m_Frames[0].JumpSquatFrame;
        set => m_Frames[0].JumpSquatFrame = value;

    }


    /// the current jump squat frame
    public float IdleTime {
        get => m_Frames[0].IdleTime;
        set => m_Frames[0].IdleTime = value;
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

    /// sets the facing direction on the xz plane
    public void SetProjectedFacingDirection(Vector3 dir) {
        var projected = Vector3.ProjectOnPlane(dir, Up);

        // if zero, use the original direction
        if (projected.sqrMagnitude > 0.0f) {
            FacingDirection = projected.normalized;
        }
    }

    // -- queries --
    /// if the state has no frames
    public bool IsEmpty {
        get => m_Frames.IsEmpty;
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
    public sealed class Frame: IEquatable<Frame> {
        // -- props --
        /// the world position
        public Vector3 Position;

        /// the character's velocity
        public Vector3 Velocity = Vector3.zero;

        /// how much velocity changed since last update
        public Vector3 Acceleration = Vector3.zero;

        /// the current facing direction
        public Vector3 FacingDirection;

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

        // -- lifetime --
        /// create an empty frame
        public Frame() {
        }

        /// create an initial frame
        public Frame(Vector3 position, Vector3 forward) {
            Position = position;
            FacingDirection = forward;
        }

        /// create a copy of an existing frame
        public Frame(CharacterState.Frame f) {
            Position = f.Position;
            Velocity = f.Velocity;
            Acceleration = f.Acceleration;
            FacingDirection = f.FacingDirection;
            IsGrounded = f.IsGrounded;
            IsInJumpSquat = f.IsInJumpSquat;
            IsInJumpStart = f.IsInJumpStart;
            IsOnWall = f.IsOnWall;
            Tilt = f.Tilt;
            Collision = f.Collision;
            JumpSquatFrame = f.JumpSquatFrame;
            IdleTime = f.IdleTime;
            PivotFrame = f.PivotFrame;
            PivotDirection = f.PivotDirection;
        }

        // -- queries --
        /// the character's up vector (hardcoded to Vector3.up)
        public Vector3 Up {
            get => Vector3.up;
        }

        /// the character's look rotation (facing & tilt)
        public Quaternion LookRotation {
            get {
                return Tilt * Quaternion.LookRotation(FacingDirection, Up);
            }
        }


            // -- IEquatable --
            public bool Equals(Frame o) {
            if (o == null) {
                return false;
            }

            return (
                Position == o.Position &&
                Velocity == o.Velocity &&
                Acceleration == o.Acceleration &&
                FacingDirection == o.FacingDirection &&
                IsGrounded == o.IsGrounded &&
                IsInJumpSquat == o.IsInJumpSquat &&
                IsInJumpStart == o.IsInJumpStart &&
                IsOnWall == o.IsOnWall &&
                Tilt == o.Tilt &&
                Collision == o.Collision &&
                JumpSquatFrame == o.JumpSquatFrame &&
                IdleTime == o.IdleTime &&
                PivotFrame == o.PivotFrame &&
                PivotDirection == o.PivotDirection

            );
        }
    }
}
}