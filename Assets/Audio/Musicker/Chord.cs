using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace Musicker {

/// a chord w/ a key and quality
public readonly struct Chord : IEnumerable<Tone> {
    // -- props --
    /// the chord tones
    readonly Tone[] m_Tones;

    // -- lifetime --
    /// create a chord from a list of tones
    public Chord(params Tone[] tones) {
        m_Tones = tones;
    }

    /// create a chord from a root note and a chord quality, building its tones
    public Chord(Tone root, Quality quality) {
        m_Tones = quality.TonesFromRoot(root);
    }

    // -- queries --
    /// the number of notes in this chord
    public int Length {
        get => m_Tones.Length;
    }

    /// the tone at the position
    public Tone this[int i] {
        get => m_Tones[i];
    }

    public IEnumerator<Tone> GetEnumerator() {
        return (IEnumerator<Tone>)m_Tones.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    // -- debugging --
    public override string ToString() {
        return string.Join(" ", m_Tones.Select((n) => n.ToString()));
    }
}

}