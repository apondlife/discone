using UnityEngine;

namespace ThirdPerson {

public interface CharacterInputQuery {
    /// a value when a press is not released
    public const float NotReleased = float.MaxValue;

    // -- queries --
    /// the buffer size
    public int BufferSize { get; }

    /// the move axis this frame
    public Vector3 Move { get; }

    /// the magnitude of the move input this frame
    public float MoveMagnitude { get; }

    /// if jump is pressed this frame
    public bool IsJumpPressed { get; }

    /// if jump was pressed within the buffer window
    public bool IsJumpPressedInBuffer(float buffer) {
        return IsJumpPressed && TimeSinceJumpPress <= buffer;
    }

    /// the time since the last jump press
    public float TimeSinceJumpPress { get; }

    /// the time the last jump press was released
    public float JumpPressReleasedAt { get; }

    /// if move was pressed in the past n frames
    public bool IsMoveIdle(int past = 1);

    /// if the move input is currently active
    public bool IsMoveActive {
        get => !IsMoveIdle();
    }
}

}