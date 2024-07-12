using System.Collections.Generic;
using ThirdPerson;

namespace Discone {

/// an input source for controlling characters via sequence of inputs over time
sealed class InputRecording: CharacterInputSource<InputFrame> {
    // -- props --
    /// the recorded sequence of input frames
    List<InputFrame> m_Frames = new();

    /// the position in the frame sequence
    int m_Head;

    /// if this is currently playing the recording
    bool m_IsPlaying;

    /// if this is currently looping
    readonly bool m_IsLooping;

    // -- lifetime --
    public InputRecording(bool isLooping = false) {
        m_IsLooping = isLooping;
    }

    // -- commands --
    /// clear the current recording
    public void Clear() {
        m_Frames.Clear();
        Reset();
    }

    /// reset the position in the sequence
    public void Reset() {
        m_Head = 0;
    }

    public void Record(InputFrame frame) {
        m_Frames.Add(frame);
    }

    /// start playing the recording
    public void Play() {
        m_IsPlaying = true;
    }

    /// stop playing the recording
    public void Pause() {
        m_IsPlaying = false;
    }

    // -- queries --
    /// if the recording has finished playback
    public bool IsFinished {
        get => m_Head >= m_Frames.Count;
    }

    // -- CharacterInputSource --
    public bool IsEnabled {
        get => m_IsPlaying;
    }

    public void Read(ref InputFrame frame) {
        if (m_Frames.Count == 0) {
            return;
        }

        if (IsFinished) {
            if (!m_IsLooping) {
                return;
            }

            m_Head = 0;
        }


        var recorded = m_Frames[m_Head];
        m_Head += 1;

        frame.Main = recorded.Main;
        frame.IsLoadPressed = recorded.IsLoadPressed;
    }
}

}