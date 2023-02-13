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

    // -- lifetime --
    /// create state from intial frame
    public CameraState(Frame initial) {
        // set props
        Fill(initial);
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
