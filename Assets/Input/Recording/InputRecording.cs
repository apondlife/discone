using System.Collections.Generic;
using ThirdPerson;

namespace Discone {

/// an input source for controlling characters via sequence of inputs over time
sealed class InputRecording: CharacterInputSource {
    /// the recorded sequence of input frames
    List<CharacterInput.Frame> m_Frames = new();

    /// the position in the frame sequence
    int m_Head;

    /// if this is currently playing the recording
    bool m_IsPlaying;

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

    public void Record(CharacterInput.Frame frame) {
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

    // -- CharacterInputSource --
    public bool IsEnabled {
        get => m_IsPlaying;
    }

    public CharacterInput.Frame Read() {
        if (m_Head >= m_Frames.Count) {
            return new CharacterInput.DefaultFrame();
        }

        var frame = m_Frames[m_Head];
        m_Head += 1;

        return frame;
    }
}

}