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

    // -- props --
    /// the queue of frames
    Queue<Frame> m_Frames = new Queue<Frame>(k_BufferSize);

    /// the tracked character state
    CharacterState m_CharacterState;

    /// an offset from the character pos to follow
    Vector3 m_FollowOffset;

    /// the yaw world-direction at init
    Vector3 m_ZeroYawDir;

    // -- lifetime --
    /// create state from intial frame
    public CameraState(Frame initial, CharacterState characterState, Vector3 followOffset) {
        // set props
        Fill(initial);
        m_CharacterState = characterState;
        m_FollowOffset = followOffset;

        // set zero values
        m_ZeroYawDir = Vector3.ProjectOnPlane(-TargetForward, Vector3.up).normalized;

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
    public Vector3 ZeroYawDir {
        get => m_ZeroYawDir;
    }

    /// .
    public Vector3 FollowPosition {
        get => m_CharacterState.Curr.Position + m_FollowOffset;
    }

    /// .
    public Vector3 TargetForward {
        get => m_CharacterState.Curr.Forward;
    }

    /// .
    public CharacterState Character {
        get => m_CharacterState;
    }

    /// converts next spherical coordinates into cartesian coordinates
    public Vector3 IntoPosition() {
        // calc dest forward from yaw
        var yawRot = Quaternion.AngleAxis(Next.Spherical.Azimuth, Vector3.up);
        var yawFwd = yawRot * m_ZeroYawDir;

        // rotate pitch on the plane containing the target's forward and up
        var pitchRot = Quaternion.AngleAxis(
            Next.Spherical.Zenith,
            Vector3.Cross(yawFwd, Vector3.up).normalized
        );

        return FollowPosition + pitchRot * yawFwd * Next.Spherical.Radius;
    }

    /// converts current position into spherical coordinates
    public Spherical IntoSpherical() {
        var currDir = Curr.Pos - FollowPosition;
        var currFwd = Vector3.ProjectOnPlane(currDir, Vector3.up);

        var radius = currDir.magnitude;

        // get current yaw
        var yaw = Vector3.SignedAngle(
            ZeroYawDir,
            currFwd,
            Vector3.up);

        var pitch = Mathf.Rad2Deg * Mathf.Atan2(currDir.y, currFwd.magnitude);

        var spherical = new Spherical();
        spherical.Radius = radius;
        spherical.Azimuth = yaw;
        spherical.Zenith = pitch;

        return spherical;
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
        /// the world position
        public Vector3 Pos;

        /// the ideal local spherical position (w/o collision)
        public Spherical Spherical;

        /// the sphecial velocity
        public Spherical Velocity;

        /// if the camera is in tracking mode
        public bool IsTracking;

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
