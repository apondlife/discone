using System;
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
    Queue<Frame> m_Frames = new Queue<Frame>(k_BufferSize);

    /// an offset from the character pos to follow
    Vector3 m_FollowOffset;

    /// the yaw world-direction at init
    Vector3 m_FollowYawZeroDir;

    // -- lifetime --
    /// create state from intial frame
    public CameraState(
        Frame initial,
        Vector3 followOffset,
        CharacterState characterState
    ) {
        // set deps
        m_CharacterState = characterState;

        // set props
        Fill(initial);
        m_FollowOffset = followOffset;

        // set zero values
        m_FollowYawZeroDir = Vector3.ProjectOnPlane(-FollowForward, Vector3.up).normalized;
    }

    // -- commands --
    /// snapshot the current state
    public void Snapshot() {
        m_Frames.Add(m_Frames[0].Copy());
    }

    /// fill the queue with the frame
    public void Fill(CameraState.Frame frame) {
        m_Frames.Fill(frame);
    }

    /// force the current frame
    public void Force(CameraState.Frame frame) {
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

    /// the current position on the camera sphere in world coords
    public Vector3 IntoIdealPosition() {
        return SphericalIntoWorld(Next.Spherical);
    }

    /// the destination position on the camera sphere in world coords
    public Vector3 IntoIdealDestPosition() {
        return SphericalIntoWorld(Next.DestSpherical);
    }

    /// converts current position into spherical coordinates
    public Spherical IntoCurrSpherical() {
        var currDir = Curr.Pos - FollowPosition;
        var currFwd = Vector3.ProjectOnPlane(currDir, Vector3.up);

        var radius = currDir.magnitude;

        var yaw = Vector3.SignedAngle(
            m_FollowYawZeroDir,
            currFwd,
            Vector3.up
        );

        var pitch = Mathf.Rad2Deg * Mathf.Atan2(
            currDir.y,
            currFwd.magnitude
        );

        return new Spherical(radius, yaw, pitch);
    }

    /// converts a local spherical position into a world position
    Vector3 SphericalIntoWorld(Spherical spherical) {
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

    // -- types --
    /// a single frame of character state
    [Serializable]
    public sealed partial class Frame: IEquatable<Frame> {
        // -- props --
        /// the actual world position post-collision
        public Vector3 Pos;

        /// the destination world position post-collision
        public Vector3 DestPos;

        /// the ideal spherical local position
        public Spherical Spherical;

        /// the ideal destination spherical local position
        public Spherical DestSpherical;

        /// the sphecial velocity
        public Spherical Velocity;

        /// if the camera is in free look mode
        public bool IsFreeLook;

        /// if the camera is colliding with something
        public bool IsColliding;

        // -- lifetime --
        /// create an empty frame
        public Frame() {
        }

        // -- factories --
        /// create a copy of this frame
        public Frame Copy() {
            var copy = new Frame(this);
            return copy;
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
