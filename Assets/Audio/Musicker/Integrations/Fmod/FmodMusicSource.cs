#nullable enable

using UnityEngine;
using System.Linq;
using System.Collections.Generic;

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
    public FMODEvent PlayLine(Line line, float delay = 0.0f, Key? key = null, FMODParams? extraParams = null) {
        FMODEvent e = PlayNote(line.Curr(), delay, key, extraParams);
        line.Advance();
        return e;
    }

    /// play the current chord in a progression and advance it
    public IEnumerable<FMODEvent> PlayProgression(Progression prog, float interval = 0.0f, Key? key = null, FMODParams? extraParams = null) {
        IEnumerable<FMODEvent> es = PlayChord(prog.Curr(), interval, key, extraParams);
        prog.Advance();
        return es;
    }

    /// play the clips in the chord; pass an interval to arpeggiate
    public IEnumerable<FMODEvent> PlayChord(Chord chord, float interval = 0.0f, Key? key = null, FMODParams? extraParams = null) {
        return chord.Select((Tone t, int i) => PlayNote(t, interval * i, key, extraParams));
    }

    /// play the note
    public FMODEvent PlayNote(Tone tone, float delay = 0.0f, Key? key = null, FMODParams? extraParams = null) {
        // transpose if necessary
        var keyed = tone;
        if (key != null) {
            keyed = key.Value.Transpose(tone);
        }

        FMODEvent e = new FMODEvent {
            emitter = m_Emitter,
            parameters = {
                [k_ParamTone] = keyed.Steps,
                [k_ParamDelay] = delay
            }
        };

        if (extraParams != null) {
            foreach (string p in extraParams.Keys) {
                e.parameters[p] = extraParams[p];
            }
        }

        return e;
    }
}

}