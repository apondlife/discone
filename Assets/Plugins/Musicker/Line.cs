namespace Musicker {

/// a note progression
/// TODO: it'd be nice to be able to just have a Run of mixed Tones and Chords
public sealed class Line {
    // -- props --
    /// the index of the current tone
    int m_Curr;

    /// the tones in this progression
    readonly Tone[] m_Tones;

    // -- lifetime --
    /// create a new line
    public Line(params Tone[] tones) {
        m_Curr = 0;
        m_Tones = tones;
    }

    // -- commands --
    /// move to the next tone
    public void Advance() {
        var next = m_Curr + 1;
        m_Curr = next % Length;
    }

    // -- queries --
    /// the current tone
    public Tone Curr() {
        return m_Tones[m_Curr];
    }

    /// the length of the line
    public int Length {
        get => m_Tones.Length;
    }

    /// the tone at the position
    public Tone this[int i] {
        get => m_Tones[i];
    }

    // -- debugging --
    public override string ToString() {
        return $"[{string.Join(",", m_Tones)}]";
    }
}

}