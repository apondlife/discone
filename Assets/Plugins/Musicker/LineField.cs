using System;
using UnityEngine;

namespace Musicker {

/// a line described with notation
[Serializable]
public sealed class LineField {
    // -- constants --
    /// the sharp character
    const char k_Sharp = '#';

    /// the flat character
    const char k_Flat = 'b';

    /// the octave character
    const char k_Octave = '\'';

    /// the quality characters
    static readonly char[] k_Quality = new char[3] {k_Flat, k_Sharp, k_Octave};

    // -- fields --
    [Tooltip("the notated line")]
    [SerializeField] string m_Notation;

    // -- props --
    /// if this is parsed
    bool m_IsParsed;

    /// the underlying line
    Line m_Line;

    // -- commands --
    /// parse the notation into a line
    void ParseLine() {
        var notes = m_Notation.Split(' ');

        var tones = new Tone[notes.Length];
        for (var i = 0; i < notes.Length; i++) {
            var note = notes[i];

            // split the root and accidentals
            var j = note.IndexOfAny(k_Quality);

            var root = note;
            var quality = "";

            if (j != -1) {
                root = note.Substring(0, j);
                quality = note.Substring(j);
            }

            // split the offset and octave
            var parsed = root == "R" ? 0 : int.Parse(root) - 1;
            var offset = parsed % 7;
            var octave = parsed / 7;

            // get steps from base tone
            var steps = 0;
            switch (offset) {
            case 0:
                steps = 0; break;
            case 1:
                steps = 2; break;
            case 2:
                steps = 4; break;
            case 3:
                steps = 5; break;
            case 4:
                steps = 7; break;
            case 5:
                steps = 9; break;
            case 6:
                steps = 11; break;
            }

            // adjust by octave
            steps += octave * 12;

            // adjust by quality
            foreach (var accidental in quality) {
                switch (accidental) {
                case k_Flat:
                    steps -= 1; break;
                case k_Sharp:
                    steps += 1; break;
                case k_Octave:
                    steps += 12; break;
                default:
                    Debug.Assert(false, $"LineField: {accidental} is not a valid accidental"); break;
                }
            }

            // add the tone
            tones[i] = new Tone(steps);
        }

        m_Line = new Line(tones);
        m_IsParsed = true;
    }

    // -- queries --
    /// the underlying line
    public Line Val {
        get {
            if (!m_IsParsed) {
                ParseLine();
            }

            return m_Line;
        }
    }
}

}