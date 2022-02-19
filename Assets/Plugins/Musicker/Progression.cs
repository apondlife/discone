namespace Musicker {

/// a chord progression
public sealed class Progression {
    // -- props --
    /// the index of the current chord
    int m_Curr;

    /// the chords in this progression
    readonly Chord[] m_Chords;

    // -- lifetime --
    /// create a new progression
    public Progression(params Chord[] chords) {
        m_Curr = 0;
        m_Chords = chords;
    }

    // -- commands --
    /// move to the next chord
    public void Advance() {
        var next = m_Curr + 1;
        m_Curr = next % m_Chords.Length;
    }

    // -- queries --
    /// get the current chord
    public Chord Curr() {
        return m_Chords[m_Curr];
    }
}

}