using System;
using UnityEngine;
using ThirdPerson;

namespace Discone {

/// an input source for controlling characters via sequence of inputs over time
[Serializable]
public sealed class SequenceInputSource: CharacterInputSource {

    [Serializable]
    private class Sequence {
        [SerializeField] int m_TotalFrames;

        [SerializeField] SerializedDictionary<int, Vector3> m_MoveSequence;
        [SerializeField] SerializedDictionary<int, bool> m_CrouchSequence;
        [SerializeField] SerializedDictionary<int, bool> m_JumpSequence;

        // -- commands --
        public int TotalFrames {
            get => m_TotalFrames;
        }

        public bool TryGetMove(int frame, out Vector3 move) {
            move = Vector3.zero;
            return m_MoveSequence.TryGetValue(frame, out move);
        }

        public bool TryGetCrouch(int frame, out bool crouch) {
            crouch = false;
            return m_CrouchSequence.TryGetValue(frame, out crouch);
        }

        public bool TryGetJump(int frame, out bool jump) {
            jump = false;
            return m_JumpSequence.TryGetValue(frame, out jump);
        }
    }

    // -- refs --
    [Header("refs")]
    [Tooltip("the sequence of inputs")]
    [SerializeField] Sequence m_Sequence;

    // the current frame this input is in
    int m_CurrentFrame;

    // the last read crouch input
    bool m_CurrentCrouchInput;

    // the last read jump input
    bool m_CurrentJumpInput;

    // the last read move input
    Vector3 m_CurrentMoveInput;

    public bool IsEnabled {
        get => true;
    }

    public CharacterInput.Frame Read() {
        // read frames from sequence
        if (m_Sequence.TryGetMove(m_CurrentFrame, out var move)) {
            m_CurrentMoveInput = move;
        }

        if (m_Sequence.TryGetCrouch(m_CurrentFrame, out var crouch)) {
            m_CurrentCrouchInput = crouch;
        }

        if (m_Sequence.TryGetJump(m_CurrentFrame, out var jump)) {
            m_CurrentJumpInput = jump;
        }


        m_CurrentFrame = (m_CurrentFrame + 1) % m_Sequence.TotalFrames;

        // produce a new frame
        return new CharacterInput.DefaultFrame(
            m_CurrentMoveInput,
            m_CurrentJumpInput,
            m_CurrentCrouchInput
        );

    }
}

}