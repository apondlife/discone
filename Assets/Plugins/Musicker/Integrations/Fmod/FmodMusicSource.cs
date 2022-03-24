using UnityEngine;
using UnityAtoms.BaseAtoms;

namespace Musicker {

/// plays music using fmod events
[RequireComponent(typeof(FMODUnity.StudioEventEmitter))]
public sealed class FmodMusicSource: MonoBehaviour {
    // -- constants --
    /// the name of the tone param
    const string k_ParamTone = "Tone";

    /// the name of the delay param
    const string k_ParamDelay = "Delay";

    // -- references --
    [Header("references")]
    [Tooltip("the fmod event emitter for this source")]
    [SerializeField] FMODUnity.StudioEventEmitter m_Emitter;

    // -- lifecycle --
    void Awake() {
        // grab references
        if (m_Emitter == null) {
            m_Emitter = GetComponent<FMODUnity.StudioEventEmitter>();
        }
    }

    // -- commands --
    /// play the current tone in the line and advance it
    public void PlayLine(Line line, float delay = 0.0f, Key? key = null) {
        PlayNote(line.Curr(), delay, key);
        line.Advance();
    }

    /// play the current chord in a progression and advance it
    public void PlayProgression(Progression prog, float interval = 0.0f, Key? key = null) {
        PlayChord(prog.Curr(), interval, key);
        prog.Advance();
    }

    /// play the clips in the chord; pass an interval to arpeggiate
    public void PlayChord(Chord chord, float interval = 0.0f, Key? key = null) {
        for (var i = 0; i < chord.Length; i++) {
            PlayNote(chord[i], interval * i, key);
        }
    }

    /// play the note
    public void PlayNote(Tone tone, float delay = 0.0f, Key? key = null) {
        // transpose if necessary
        var keyed = tone;
        if (key != null) {
            keyed = key.Value.Transpose(tone);
        }

        // play the event for this note
        m_Emitter.Play();
        m_Emitter.SetParameter(k_ParamTone, keyed.Steps);
        m_Emitter.SetParameter(k_ParamDelay, delay);
    }
}

}