using UnityEngine;

namespace Musicker {

/// helpers for parsing notation
static class Notation {
    // -- constants --
    /// the sequence separator
    const char k_SeqSep = ' ';

    /// the sharp character
    const char k_Sharp = '#';

    /// the flat character
    const char k_Flat = 'b';

    /// the octave character
    const char k_Octave = '\'';

    /// the tone quality characters
    static readonly char[] k_ToneQuality = new char[3] {
        k_Flat,
        k_Sharp,
        k_Octave
    };

    /// the chord separator
    const char k_ChordSep = '.';

    /// the major 5th marker
    const string k_Maj5 = "M5";

    /// the minor 5th marker
    const string k_Min5 = "m5";

    /// the major 7th marker
    const string k_Maj7 = "M7";

    /// the dominant 7th marker
    const string k_Dom7 = "7";

    /// the minor 7th marker
    const string k_Min7 = "m7";

    /// the half-diminished, minor 7 flat 5, marker
    const string k_Min7Flat5 = "m7b5";

    /// the diminished marker
    const string k_Dim7 = "dim7";

    // -- queries --
    /// decode a tone from notation
    public static Tone DecodeTone(string notation) {
        // split the root and accidentals
        var j = notation.IndexOfAny(k_ToneQuality);

        var root = notation;
        var quality = "";

        if (j != -1) {
            root = notation.Substring(0, j);
            quality = notation.Substring(j);
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
        foreach (var marker in quality) {
            switch (marker) {
            case k_Flat:
                steps -= 1; break;
            case k_Sharp:
                steps += 1; break;
            case k_Octave:
                steps += 12; break;
            default:
                Debug.Assert(false, $"notation: {marker} is not a quality marker"); break;
            }
        }

        // produce the tone
        return new Tone(steps);
    }

    /// decode a run of tones from notation
    public static Tone[] DecodeTones(string[] notes) {
        var tones = new Tone[notes.Length];
        for (var i = 0; i < notes.Length; i++) {
            tones[i] = DecodeTone(notes[i]);
        }

        return tones;
    }

    /// decode a chord from notation
    public static Chord DecodeChord(string notation) {
        var parts = notation.Split(k_ChordSep);

        // if this might have a quality
        if (parts.Length == 2) {
            var quality = DecodeQuality(parts[1]);

            // and it does, treat it as root & quality
            if (quality.Length != 0)  {
                return new Chord(
                    DecodeTone(parts[0]),
                    quality
                );
            }
        }

        // otherwise, treat it as notes
        return new Chord(DecodeTones(parts));
    }

    /// decode a quality from notation
    public static Quality DecodeQuality(string notation) {
        switch (notation) {
        case k_Maj5:
            return Quality.Maj5;
        case k_Min5:
            return Quality.Min5;
        case k_Maj7:
            return Quality.Maj7;
        case k_Dom7:
            return Quality.Dom7;
        case k_Min7:
            return Quality.Min7;
        case k_Min7Flat5:
            return Quality.Min7Flat5;
        case k_Dim7:
            return Quality.Dim7;
        default:
            return default;
        }
    }

    /// decode a line from notation
    public static Line DecodeLine(string notation) {
        var notes = notation.Split(k_SeqSep);
        return new Line(DecodeTones(notes));
    }

    /// decode a progression from notation
    public static Progression DecodeProgression(string notation) {
        var notes = notation.Split(k_SeqSep);

        var chords = new Chord[notes.Length];
        for (var i = 0; i < notes.Length; i++) {
            chords[i] = DecodeChord(notes[i]);
        }

        return new Progression(chords);
    }
}

}