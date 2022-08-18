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

    // -- commands --
    /// snapshot the current state
    public void Snapshot() {
        m_Frames.Add(m_Frames[0].Copy());
    }

    /// fill the queue with the frame
    public void Fill(CharacterState.Frame frame) {
        m_Frames.Fill(frame);
    }

    /// force the current frame
    public void Force(CharacterState.Frame frame) {
        m_Frames[0] = frame;
    }

    // -- queries --
    /// the current frame
    public Frame Curr {
        get => m_Frames[0];
    }

    /// the previous frame
    public Frame Prev {
        get => m_Frames[1];
    }

    /// if the state has no frames
    public bool IsEmpty {
        get => m_Frames.IsEmpty;
    }

    /// if currently grounded
    public bool IsGrounded {
        get => Curr.IsGrounded;
    }

    /// if currently idle
    public bool IsIdle {
        get => Curr.IsIdle;
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

        /// how much the velocity changed since last frame
        public Vector3 Acceleration = Vector3.zero;

        /// the current facing direction
        public Vector3 Forward = Vector3.forward;

        /// if the character is in jump squat
        public bool IsInJumpSquat = false;

        /// if the character is in its first jump frame
        public bool IsInJumpStart = false;

        /// if the characer is on the wall
        public bool IsOnWall = false;

        /// how much tilted the character is
        public Quaternion Tilt = Quaternion.identity;

        /// the ground collision for the previous frame
        public CharacterCollision Ground;

        /// the wall collision for the previous frame
        public CharacterCollision Wall;

        /// the current frame in the jump squat
        public int JumpSquatFrame = -1;

        /// the current time the character hasn't moved
        public float IdleTime = 0.0f;

        ///  the current frame in pivot animation
        public int PivotFrame = -1;

        ///  the direction of the current pivot
        public Vector3 PivotDirection = Vector3.zero;

        /// the number of jumps executed since last grounded
        public uint Jumps = 0;

        /// the container of events that happened this frame
        public CharacterEventSet Events;

        // -- lifetime --
        /// create an empty frame
        public Frame() {
        }

        /// create an initial frame
        public Frame(Vector3 position, Vector3 forward) {
            Position = position;
            Forward = forward;
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
        /// the character's up vector (hardcoded to Vector3.up)
        public Vector3 Up {
            get => Vector3.up;
        }

        /// the normal in relation to the current surface
        public Vector3 Normal {
            get => Ground.Normal;
        }

        /// if the character is grounded
        public bool IsGrounded {
            get => !Ground.IsNone;
        }

        /// if currently idle
        public bool IsIdle {
            get => IdleTime > 0.0f;
        }

        /// the character's look rotation (facing & tilt)
        public Quaternion LookRotation {
            get => Tilt * Quaternion.LookRotation(Forward, Up);
        }

        /// the velocity on the xz-plane
        public Vector3 PlanarVelocity {
            get => Velocity.XNZ();
        }

        /// the velocity in the ground plane, or planar velocity if not grounded
        public Vector3 GroundVelocity {
            get {
                if (Ground.IsNone) {
                    return PlanarVelocity;
                } else {
                    return Vector3.ProjectOnPlane(Velocity, Ground.Normal);
                }
            }
        }

        // -- factories --
        /// create a copy of this frame
        public Frame Copy() {
            var copy = new Frame(this);
            copy.Events.Clear();
            return copy;
        }
    }
}
}