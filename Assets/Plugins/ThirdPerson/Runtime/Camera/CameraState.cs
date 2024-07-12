using System;
using Soil;
using UnityEngine;

namespace ThirdPerson {

/// the character's authoritative state
public sealed partial class CameraState {
    // -- constants --
    #if UNITY_EDITOR
    const uint k_BufferSize = 10;
    #else
    const uint k_BufferSize = 5;
    #endif

    // -- deps --
    /// the tracked character state
    CharacterState m_CharacterState;

    // -- props --
    /// the queue of frames
    readonly Ring<Frame> m_Frames = new(k_BufferSize);

    /// an offset from the character pos to follow
    Vector3 m_FollowOffset;

    /// the yaw world-direction at init
    Vector3 m_FollowYawZeroDir;

    /// the pos of the current hit surface
    Vector3 m_HitPos;

    /// the normal of the current hit surface
    Vector3 m_HitNormal;

    // -- lifetime --
    /// create state from intial frame
    public CameraState(
        Vector3 followOffset,
        CharacterState characterState
    ) {
        // set deps
        m_CharacterState = characterState;

        // set props
        m_FollowOffset = followOffset;

        // fill with copies of the initial frame
        for (var i = 0; i < m_Frames.Length; i++) {
            m_Frames[i] = new Frame();
        }

        // set zero values
        m_FollowYawZeroDir = Vector3.ProjectOnPlane(-FollowForward, Vector3.up).normalized;
    }

    // -- commands --
    /// push the next frame from the current state
    public void Advance() {
        // advance to the next frame
        m_Frames.Offset();

        // and update it to match the current frame
        var next = m_Frames[0];
        next.Assign(m_Frames[1]);
    }

    /// override the current frame
    public void Override(Frame frame) {
        m_Frames[0] = frame;
    }

    // -- queries --
    /// .
    public CharacterState Character {
        get => m_CharacterState;
    }

    /// the follow target's current position
    public Vector3 FollowPosition {
        get => m_CharacterState.Next.Position + m_FollowOffset;
    }

    /// the follow target's current forward dir
    public Vector3 FollowForward {
        get => m_CharacterState.Next.Forward;
    }

    /// .
    public Vector3 FollowYawZeroDir {
        get => m_FollowYawZeroDir;
    }

    /// the pos of the current hit surface
    public Vector3 ClipPos {
        get => Next.IsColliding ? HitPos : Next.Pos;
    }

    /// the normal of the current hit surface
    public Vector3 ClipNormal {
        get => Next.IsColliding ? HitNormal : Next.Forward;
    }

    /// the current position on the camera sphere in world coords
    public Vector3 IntoIdealPosition() {
        return SphericalIntoWorld(Next.Spherical);
    }

    /// converts current position into spherical coordinates
    public Spherical IntoCurrSpherical() {
        return WorldIntoSpherical(Curr.Pos);
    }

    /// converts a world position into a local spherical position
    public Spherical WorldIntoSpherical(Vector3 pos) {
        var delta = pos - FollowPosition;
        var forward = Vector3.ProjectOnPlane(delta, Vector3.up);

        var radius = delta.magnitude;

        var yaw = Vector3.SignedAngle(
            m_FollowYawZeroDir,
            forward,
            Vector3.up
        );

        var pitch = Mathf.Rad2Deg * Mathf.Atan2(
            delta.y,
            forward.magnitude
        );

        return new Spherical(radius, yaw, pitch);
    }

    /// converts a local spherical position into a world position
    public Vector3 SphericalIntoWorld(Spherical spherical) {
        // calc dest forward from yaw
        var yawRot = Quaternion.AngleAxis(
            spherical.Azimuth,
            Vector3.up
        );

        var yawFwd = yawRot * m_FollowYawZeroDir;

        // rotate pitch on the plane containing the target's forward and up
        var pitchRot = Quaternion.AngleAxis(
            spherical.Zenith,
            Vector3.Cross(yawFwd, Vector3.up).normalized
        );

        return FollowPosition + pitchRot * yawFwd * spherical.Radius;
    }

    /// the buffer size
    public uint BufferSize {
        get => k_BufferSize;
    }

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

    // -- props/hot --
    // TODO: unclear why these were on collision system and not on frame
    /// the pos of the current hit surface
    public Vector3 HitPos {
        get => m_HitPos;
        set => m_HitPos = value;
    }

    /// the normal of the current hit surface
    public Vector3 HitNormal {
        get => m_HitNormal;
        set => m_HitNormal = value;
    }

    // -- types --
    /// a single frame of character state
    [Serializable]
    public sealed partial class Frame: IEquatable<Frame> {
        // -- props --
        /// the camera's forward direction
        public Vector3 Forward;

        /// the camera's up direction
        public Vector3 Up;

        /// the actual world position post-collision
        public Vector3 Pos;

        /// the ideal spherical local position
        public Spherical Spherical;

        /// the spherical velocity
        public Spherical Velocity;

        /// the camera's field of view
        public float Fov;

        /// if the camera is in free look mode
        public bool IsFreeLook;

        /// if the camera is colliding with something
        public bool IsColliding;

        /// the tilt (dutch) angle
        public float Tilt;

        /// the tilt (dutch) angle speed
        public float TiltSpeed;

        // -- lifetime --
        /// create an empty frame
        public Frame() {
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