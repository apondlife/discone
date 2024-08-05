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
    readonly CharacterTuning m_Tuning;

    // -- props --
    /// the queue of frames
    readonly Ring<Frame> m_Frames = new(k_BufferSize);

    /// the interpolated frame
    readonly Frame m_Interpolated = new();

    // -- lifetime --
    /// create state from initial frame and dependencies
    public CharacterState(
        Vector3 position,
        Vector3 forward,
        CharacterTuning tuning
    ) {
        // set deps
        m_Tuning = tuning;

        // fill with copies of the initial frame
        for (var i = 0; i < m_Frames.Length; i++) {
            m_Frames[i] = Create(position, forward);
        }
    }

    // -- commands --
    /// initialize a frame from a forward and position
    public void InitFrame(
        Frame frame,
        Vector3 position,
        Vector3 forward
    ) {
        if (forward == Vector3.zero) {
            Log.Character.W($"can't set a zero forward vector, ignoring");
            return;
        }

        // create a minimal frame
        frame.Position = position;
        frame.Forward = forward;

        // init any properties from tuning
        InitFrame(frame);
    }

    /// initialize a frame from the tuning
    public void InitFrame(Frame frame) {
        frame.Surface_Drag = m_Tuning.Friction_SurfaceDrag;
        frame.Surface_KineticFriction = m_Tuning.Friction_Kinetic;
        frame.Surface_StaticFriction = m_Tuning.Friction_Static;
    }

    /// push the next frame from the current state
    public void Advance() {
        // advance the frame
        m_Frames.Offset();

        // update it to match the current frame w/ no forces or events
        var next = m_Frames[0];
        next.Assign(m_Frames[1]);
        next.Force.Clear();
        next.Events.Clear();
    }

    /// override the current frame
    public void Override(Frame frame) {
        m_Frames[0] = frame;
    }

    /// interpolates the frame between Curr and Next, by factor of k
    public void Interpolate(float k) {
        m_Interpolated.Interpolate(
            Curr,
            Next,
            k
        );
    }

    // -- factories --
    /// create a new frame with a forward and position
    Frame Create(
        Vector3 position,
        Vector3 forward
    ) {
        var frame = new Frame();
        InitFrame(frame, position, forward);
        return frame;
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

    /// the interpolated frame
    public Frame Interpolated {
        get => m_Interpolated;
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

    /// the power of the currently charging jump
    public float NextJumpPower {
        get => Next.IsInJumpSquat ? m_Tuning.JumpById(Next.NextJump).Power(Next.JumpState.PhaseElapsed) : 0f;
    }

    /// the buffer size
    public uint BufferSize {
        get => k_BufferSize;
    }

    /// gets the frame at offset
    public Frame this[int offset] {
        get => m_Frames[offset];
    }

    // -- types --
    /// a single frame of character state
    [Serializable]
    public sealed partial class Frame: IEquatable<Frame> {
        // -- props --
        /// the world position
        public Vector3 Position;

        /// the character's velocity
        public Vector3 Velocity;

        /// the conserved speed into the main surface
        public float Inertia;

        /// the input force this frame
        public CharacterForce Force;

        /// how much the velocity changed since last frame
        public Vector3 Acceleration;

        /// the facing direction
        public Vector3 Forward;

        /// if the character is in jump squat
        public bool IsInJumpSquat = false;

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
        public float IdleTime;

        /// the drag for surface movement
        public float Surface_Drag;

        /// the kinetic friction for surface movement
        public float Surface_KineticFriction;

        /// the static friction for surface movement
        public float Surface_StaticFriction;

        /// the direction of the current pivot
        public Vector3 PivotDirection;

        /// the id of the next (to-execute) jump
        public JumpId NextJump;

        /// the id of the active (in-progress) jump
        public JumpId ActiveJump;

        /// the buffered surface to jump from;
        public CharacterCollision JumpSurface;

        /// the coyote time remaining
        public float CoyoteTime = 0;

        /// the time since last jump triggered
        public float Jump_Elapsed;

        /// the time jump was released at (in relation to jump press)
        public float Jump_ReleasedAt;

        /// the cooldown time remaining
        public float Jump_CooldownElapsed;

        /// the cooldown time duration
        public float Jump_CooldownDuration;

        /// the container of events that happened this frame
        public CharacterEventSet Events;

        // -- lifetime --
        /// create an empty frame
        public Frame() {
        }

        // -- commands --
        // TODO: extract placement into thirdperson?
        /// assign the frame from the placement
        public void Assign(
            Vector3 position,
            Vector3 forward
        ) {
            Position = position;
            Forward = forward;
        }

        /// assign this frame to an interpolation of src and dst
        public void Interpolate(
            Frame src,
            Frame dst,
            float k
        ) {
            k = Mathf.Clamp01(k);

            // by default, the values are just taken from the end
            Assign(dst);

            // interpolate relevant values
            // TODO: should bool do something different? past 50%?
            Position = Vector3.Lerp(src.Position, dst.Position, k);
            Velocity = Vector3.Lerp(src.Velocity, dst.Velocity, k);
            Force.Interpolate(src.Force, dst.Force, k);
            Acceleration = Vector3.Lerp(src.Acceleration, dst.Acceleration, k);
            Forward = Vector3.Slerp(src.Forward, dst.Forward, k);
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
            get => Quaternion.LookRotation(Forward, Up);
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
        public Vector3 PlanarAcceleration {
            get => Acceleration.XNZ();
        }

        /// the velocity direction along the xz-plane
        public Vector3 PlanarDirection {
            get => AsDirection(PlanarVelocity);
        }

        /// the velocity in the main surface plane, or total velocity if none
        public Vector3 SurfaceVelocity {
            get => OnSurface(Velocity);
        }

        /// the force in the main surface plane, or total force if none
        [Obsolete("don't use this; always consider if impulse is relevant and use OnSurface")]
        public Vector3 SurfaceForce {
            get => OnSurface(Force.Continuous);
        }

        /// the acceleration in the main surface plane, or total acceleration if none
        public Vector3 SurfaceAcceleration {
            get => OnSurface(Acceleration);
        }

        /// the velocity direction along the surface
        public Vector3 SurfaceDirection {
            get => AsDirection(SurfaceVelocity);
        }

        /// get the vector projected into the surface, if any
        public Vector3 OnSurface(Vector3 vector) {
            if (MainSurface.IsNone) {
                return vector;
            }

            return Vector3.ProjectOnPlane(vector, MainSurface.Normal);
        }

        /// treat the vector as a direction, falling back to forward
        public Vector3 AsDirection(Vector3 sourceDir) {
            if (sourceDir == Vector3.zero) {
                return Forward;
            }

            return sourceDir.normalized;
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