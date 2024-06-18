using UnityEngine;

namespace ThirdPerson {

public interface CharacterInputQuery {
    // -- queries --
    /// the move axis this frame
    public Vector3 Move { get; }

    /// the magnitude of the move input this frame
    public float MoveMagnitude { get; }

    /// if jump is pressed this frame
    public bool IsJumpPressed { get; }

    /// if jump was pressed within the buffer window
    public bool IsJumpDown(float buffer);

    /// if move was pressed in the past n frames
    public bool IsMoveIdle(int past = 1);

    /// the buffer size
    public int BufferSize { get; }

    /// if the move input is currently active
    public bool IsMoveActive {
        get => !IsMoveIdle();
    }
}

}