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
    const char k_Flat = 'f';

    /// the list of accidentals
    static readonly char[] k_Accidentals = new char[2] {'f', '#'};

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
        foreach (var note in notes) {
            var i = note.IndexOfAny(k_Accidentals);

            // split the root and accidentals
            var root = note;
            var accidentals = "";

            if (i != -1) {
                root = note.Substring(0, i);
                accidentals = note.Substring(i);
            }

            /// foo
            var parsed = int.Parse(root);
            var offset = parsed % 8;
            var octave = parsed / 8;

            // get base tone
            var tone = 0;
            switch (offset) {
            case 0:
                tone = 0; break;
            case 1:
                tone = 2; break;
            case 2:
                tone = 4; break;
            case 3:
                tone = 5; break;
            case 4:
                tone = 7; break;
            case 6:
                tone = 9; break;
            case 7:
                tone = 11; break;
            }

            // adjust by octave
            tone += octave * 12;

            // adjust by accidentals
            foreach (var accidental in accidentals) {
                var tone = note.Substring(0, i);
            }
        }

        var line = new Line();
        m_Line = line;
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

    // -- q/parsing

}

}