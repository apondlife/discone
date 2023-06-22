using System;
using UnityEngine;

namespace ThirdPerson {

/// the character's authoritative state
public sealed partial class CharacterState {
    // -- constants --
    #if UNITY_EDITOR
    const uint k_BufferSize = 60 * 5;
    #else
    const uint k_BufferSize = 5;
    #endif

    // -- deps --
    /// the tuning
    CharacterTuning m_Tuning;

    // -- props --
    /// the queue of frames
    Queue<Frame> m_Frames = new Queue<Frame>(k_BufferSize);

    // -- lifetime --
    /// create state from intial frame and dependencies
    public CharacterState(
        Frame initial,
        CharacterTuning tuning
    ) {
        // set deps
        m_Tuning = tuning;

        // set props
        Fill(initial);
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
    /// the next frame
    public Frame Next {
        get => m_Frames[0];
    }

    /// the current frame
    public Frame Curr {
        get => m_Frames[1];
    }

    /// the previous frame
    public Frame Prev {
        get => m_Frames[2];
    }

    /// if the state has no frames
    public bool IsEmpty {
        get => m_Frames.IsEmpty;
    }

    /// if currently idle
    public bool IsIdle {
        get => Next.IsIdle;
    }

    /// if the ground speed this frame is below the movement threshold
    public bool IsStopped {
        get => Next.GroundVelocity.magnitude < m_Tuning.Horizontal_MinSpeed;
    }

    /// if the ground speed last frame was below the movement threshold
    public bool WasStopped {
        get => Curr.GroundVelocity.magnitude < m_Tuning.Horizontal_MinSpeed;
    }

    /// the buffer size
    public uint BufferSize {
        get => k_BufferSize;
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

        /// the facing direction
        public Vector3 Forward = Vector3.forward;

        /// if the character is landing
        public bool IsLanding = false;

        /// if the character is in jump squat
        public bool IsInJumpSquat = false;

        /// if the character is crouching
        public bool IsCrouching = false;

        /// how much tilted the character is
        public Quaternion Tilt = Quaternion.identity;

        /// the ground collision for the previous frame
        public CharacterCollision Ground;

        /// the wall collision for the previous frame
        public CharacterCollision Wall;

        /// the frame in the jump squat
        public int JumpSquatFrame = -1;

        /// the time the character hasn't moved
        public float IdleTime = 0.0f;

        /// the drag for grounded movement
        public float Horizontal_Drag = 0.0f;

        /// the kinectic friction for grounded movement
        public float Horizontal_KineticFriction = 0.0f;

        /// the static friction for grounded movement
        public float Horizontal_StaticFriction = 0.0f;

        /// the frame in pivot animation
        public int PivotFrame = -1;

        /// the direction of the current pivot
        public Vector3 PivotDirection = Vector3.zero;

        /// the direction fo the current crouch
        public Vector3 CrouchDirection = Vector3.zero;

        /// the number of jumps executed since last grounded
        public uint Jumps = 0;

        /// the number of coyote frames the available
        public int CoyoteFrames = 0;

        /// the number of cooldown frames available
        public int CooldownFrames = 0;

        /// the index of the current jump tuning
        public uint JumpTuningIndex = 0;

        /// the current number of jumps in the current tuning
        public uint JumpTuningJumpIndex = 0;

        /// the container of events that happened this frame
        public CharacterEventSet Events;

        // -- lifetime --
        /// create an empty frame
        public Frame() {
        }

        /// create an initial frame
        public Frame(Vector3 position, Vector3 forward) {
            Position = position;

            if (forward.magnitude == 0) {
                Debug.LogWarning("[character frame] can't set a zero forward vector, ignoring");
                return;
            }

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

        /// if the character is touching the ground (or thinks they are)
        public bool IsOnGround {
            get => Ground.IsSome || CoyoteFrames > 0;
        }

        /// the ground-like collision surface
        public CharacterCollision GroundSurface {
            get => Ground.IsSome ? Ground : Wall;
        }

        /// the wall-like collision surface
        public CharacterCollision WallSurface {
            get => Wall.IsSome ? Wall : Ground;
        }

        /// if the character is on the wall
        public bool IsOnWall {
            get => WallSurface.Angle > 0;
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

        // -- factories --
        /// interpolate a src and dst frame by some interpolant k
        public static Frame Interpolate(
            Frame start,
            Frame end,
            float k
        ) {
            var res = end.Copy();
            Interpolate(start, end, ref res, k);
            return res;
        }

        /// interpolate a src and dst frame into a result frame by some interpolant k
        [Obsolete("not osbolete, but don't call this before making sure you copied the Frame data the interpolation ends on")]
        public static void Interpolate(
            Frame start,
            Frame end,
            ref Frame res,
            float k
        ) {
            k = Mathf.Clamp01(k);

            // by default, the values are just taken from the end
            // TODO: should bools be doing something different? past 50%?
            res.Position = Vector3.Lerp(start.Position, end.Position, k);
            res.Velocity = Vector3.Lerp(start.Velocity, end.Velocity, k);
            res.Acceleration = Vector3.Lerp(start.Acceleration, end.Acceleration, k);
            res.Forward = Vector3.Slerp(start.Forward, end.Forward, k);
            res.Tilt = Quaternion.Slerp(start.Tilt, end.Tilt, k);
        }
    }

    // -- debug --
    #if UNITY_EDITOR
    /// set the current frame offset
    public void Debug_MoveHead(int offset) {
        m_Frames.Move(offset);
    }
    #endif
}

}