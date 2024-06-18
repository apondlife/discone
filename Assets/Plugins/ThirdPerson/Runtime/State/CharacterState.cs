using System;
using Soil;
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
    Ring<Frame> m_Frames = new(k_BufferSize);

    // -- lifetime --
    /// create state from initial frame and dependencies
    public CharacterState(
        Vector3 position,
        Vector3 forward,
        CharacterTuning tuning
    ) {
        // set deps
        m_Tuning = tuning;

        // fill with the initial frame
        var initial = Create(position, forward);
        Fill(initial);
    }

    // -- commands --
    /// create the next frame from the current frame
    public void Advance() {
        // create a new frame w/ no forces or events
        var next = m_Frames[0].Copy();
        next.Force = Vector3.zero;
        next.Events.Clear();

        // add the frame
        m_Frames.Add(next);
    }

    /// override the current frame
    public void Override(Frame frame) {
        if (m_Frames.IsEmpty) {
            Fill(frame);
        } else {
            m_Frames[0] = frame;
        }
    }

    /// fill the queue with the frame
    public void Fill(Frame frame) {
        m_Frames.Fill(frame);
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
        get => Curr.SurfaceVelocity.magnitude < m_Tuning.Surface_MinSpeed;
    }

    /// the buffer size
    public uint BufferSize {
        get => k_BufferSize;
    }

    /// gets the frame at offset
    public Frame this[int offset] {
        get => m_Frames[offset];
    }

    // -- factories --
    /// create a frame from forward and position
    public Frame Create(
        Vector3 position,
        Vector3 forward
    ) {
        if (forward == Vector3.zero) {
            Log.Character.W($"can't set a zero forward vector, ignoring");
            return new Frame();
        }

        // create a minimal frame
        var frame = new Frame();
        frame.Position = position;
        frame.Forward = forward;

        // init any properties from tuning
        frame.Surface_Drag = m_Tuning.Friction_SurfaceDrag;
        frame.Surface_KineticFriction = m_Tuning.Friction_Kinetic;
        frame.Surface_StaticFriction = m_Tuning.Friction_Static;

        return frame;
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

        /// the conserved speed into the main surface
        public float Inertia = 0f;

        /// the input force this frame
        public Vector3 Force = Vector3.zero;

        /// how much the velocity changed since last frame
        public Vector3 Acceleration = Vector3.zero;

        /// the facing direction
        public Vector3 Forward = Vector3.forward;

        /// if the character is landing
        public bool IsLanding = false;

        /// if the character is in jump squat
        public bool IsInJumpSquat = false;

        /// how much tilted the character is
        public Quaternion Tilt = Quaternion.identity;

        /// the collision surfaces
        // TODO: should we sort this by normal mag?
        public CharacterCollision[] Surfaces;

        /// the surface w/ the most normal force
        public CharacterCollision MainSurface;

        /// the currently perceived surface
        public CharacterCollision PerceivedSurface;

        /// the current surface transfer tangent
        public Vector3 SurfaceTangent;

        /// the time the character hasn't moved
        public float IdleTime = 0f;

        /// the drag for surface movement
        public float Surface_Drag = 0f;

        /// the kinetic friction for surface movement
        public float Surface_KineticFriction = 0f;

        /// the static friction for surface movement
        public float Surface_StaticFriction = 0f;

        /// the direction of the current pivot
        public Vector3 PivotDirection = Vector3.zero;

        /// the number of jumps executed since last grounded
        public uint Jumps = 0;

        /// the buffered surface to jump from;
        public CharacterCollision JumpSurface;

        /// the coyote time remaining
        public float CoyoteTime = 0;

        /// the cooldown time remaining
        public float Jump_CooldownElapsed = 0f;

        /// the cooldown time duration
        public float Jump_CooldownDuration = 0f;

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

        /// sets the forward direction on the xz plane
        public void SetProjectedForward(Vector3 dir) {
            var projected = Vector3.ProjectOnPlane(dir, Up);

            // if zero, use the original direction
            if (projected.sqrMagnitude > 0f) {
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
            get => MainSurface.Normal;
        }

        // TODO: IsOnSurface? checking main surface vs checking collisions?
        /// if this is colliding with anything
        public bool IsColliding {
            get => Surfaces != null && Surfaces.Length != 0;
        }

        /// if the character is touching the ground (or thinks they are)
        public bool IsOnGround {
            get => CoyoteTime > 0f;
        }

        /// if the character is on the wall
        public bool IsOnWall {
            get => MainSurface.Angle > 0f;
        }

        /// if currently idle
        public bool IsIdle {
            get => IdleTime > 0f;
        }

        /// if the character is crouching
        public bool IsCrouching {
            get => IsInJumpSquat;
        }

        /// the character's look rotation (facing & tilt)
        public Quaternion LookRotation {
            get => Tilt * Quaternion.LookRotation(Forward, Up);
        }

        /// the velocity direction
        public Vector3 Direction {
            get => AsDirection(Velocity);
        }

        /// the velocity on the xz-plane
        public Vector3 PlanarVelocity {
            get => Velocity.XNZ();
        }

        /// the velocity on the xz-plane
        public Vector3 PlanarForce {
            get => Force.XNZ();
        }

        /// the velocity direction along the xz-plane
        public Vector3 PlanarDirection {
            get => AsDirection(PlanarVelocity);
        }

        /// the velocity in the main surface plane, or raw velocity if none
        public Vector3 SurfaceVelocity {
            get {
                if (MainSurface.IsNone) {
                    return Velocity;
                } else {
                    return Vector3.ProjectOnPlane(Velocity, MainSurface.Normal);
                }
            }
        }

        /// the force in the main surface plane, or raw force if none
        public Vector3 SurfaceForce {
            get {
                if (MainSurface.IsNone) {
                    return Force;
                } else {
                    return Vector3.ProjectOnPlane(Force, MainSurface.Normal);
                }
            }
        }

        /// the velocity direction along the surface
        public Vector3 SurfaceDirection {
            get => AsDirection(SurfaceVelocity);
        }

        /// treat the vector as a direction, falling back to forward
        public Vector3 AsDirection(Vector3 sourceDir) {
            if (sourceDir == Vector3.zero) {
                return Forward;
            }

            return sourceDir.normalized;
        }

        // -- factories --
        /// create a copy of this frame
        public Frame Copy() {
            return new Frame(this);
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
            res.Force = Vector3.Lerp(start.Force, end.Force, k);
            res.Acceleration = Vector3.Lerp(start.Acceleration, end.Acceleration, k);
            res.Forward = Vector3.Slerp(start.Forward, end.Forward, k);
            res.Tilt = Quaternion.Slerp(start.Tilt, end.Tilt, k);
        }
    }

    // -- debug --
    #if UNITY_EDITOR
    /// set the current frame offset
    public void Debug_MoveHead(int offset) {
        m_Frames.Offset(offset);
    }
    #endif
}

}