using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Musicker {

/// a thing that plays music
public sealed class MusicSource: MonoBehaviour {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the max volume")]
    [SerializeField] float m_MaxVolume = 1.0f;

    [Tooltip("the template audio source")]
    [SerializeField] AudioSource m_Template;

    // -- config --
    [Header("config")]
    [Tooltip("the number of audio sources to create or keep")]
    [SerializeField] int m_NumSources = 4;

    [Tooltip("the audio source to realize sound")]
    [SerializeField] List<AudioSource> m_Sources;

    [Tooltip("the current instrument")]
    [SerializeField] Instrument m_Instrument;

    // -- props --
    /// the index of the next available audio source
    int m_NextSource = 0;

    // -- lifecycle --
    void Awake() {
        // make sure the template is one of the sources
        if (m_Template != null && !m_Sources.Contains(m_Template)) {
            m_Sources.Add(m_Template);
        }

        // create audio sources
        for (var i = m_Sources.Count; i < m_NumSources; i++) {
            m_Sources.Add(InitAudioSource());
        }
    }

    // -- commands --
    /// play the current tone in the line and advance it
    public void PlayLine(in Line line, in Key? key = null) {
        PlayTone(line.Curr(), key);
        line.Advance();
    }

    /// play the current chord in a progression and advance it
    public void PlayProgression(in Progression prog, float interval = 0.0f, in Key? key = null) {
        PlayChord(prog.Curr(), interval, key);
        prog.Advance();
    }

    /// play the clips in the chord
    public void PlayChord(in Chord chord, in Key? key = null) {
        PlayChord(chord, 0.0f, key);
    }

    /// play the clips in the chord, pass an interval to arpeggiate
    public void PlayChord(in Chord chord, float interval, in Key? key = null) {
        StartCoroutine(PlayChordAsync(chord, interval, key));
    }

    /// play the clips in the chord. pass an interval to arpeggiate.
    IEnumerator PlayChordAsync(Chord chord, float interval = 0.0f, Key? key = null) {
        for (var i = 0; i < chord.Length; i++) {
            PlayTone(chord[i], key);

            if (interval != 0.0) {
                yield return new WaitForSeconds(interval);
            }
        }
    }

    /// play the clip for a tone
    public void PlayTone(in Tone tone, in Key? key = null) {
        // transpose if necessary
        var keyed = tone;
        if (key != null) {
            keyed = key.Value.Transpose(tone);
        }

        // play the clip
        PlayClip(m_Instrument.FindClip(keyed));
    }

    /// play a random audio clip
    public void PlayRand() {
        PlayClip(m_Instrument.RandClip());
    }

    // -- c/config
    /// set the pitch
    public void SetPitch(float pitch) {
        foreach (var source in m_Sources) {
            source.pitch = pitch;
        }
    }

    /// set the maximum distance
    public void SetMaxDistance(float distance) {
        foreach (var source in m_Sources) {
            source.maxDistance = distance;
        }
    }

    /// the the max volume of the loop
    public void SetMaxVolume(float max) {
        m_MaxVolume = max;

        foreach (var source in m_Sources) {
            source.volume = Mathf.Min(source.volume, max);
        }
    }

    // -- c/helpers
    /// play a clip on the next source
    void PlayClip(AudioClip clip) {
        // play the clip
        var source = m_Sources[m_NextSource];
        source.clip = clip;
        source.Play();

        // advance the source
        m_NextSource = (m_NextSource + 1) % m_NumSources;
    }

    // -- props/hot
    /// the current instrument
    public Instrument Instrument {
        get => m_Instrument;
        set => m_Instrument = value;
    }

    // -- queries --
    /// if the musicker has any sources available
    public bool IsAvailable() {
        foreach (var source in m_Sources) {
            if (!source.isPlaying) {
                return true;
            }
        }

        return false;
    }

    // -- factories --
    /// create a new audio source
    AudioSource InitAudioSource() {
        // add the audio source
        var src = gameObject.AddComponent<AudioSource>();
        src.volume = m_MaxVolume;

        // copy templated props
        var tmp = m_Template;
        if (tmp != null) {
            src.minDistance = tmp.minDistance;
            src.maxDistance = tmp.maxDistance;

            src.rolloffMode = tmp.rolloffMode;
            src.dopplerLevel = tmp.dopplerLevel;

            var t0 = AudioSourceCurveType.CustomRolloff;
            var t1 = AudioSourceCurveType.Spread;
            for (var t = t0; t <= t1; t++) {
                src.SetCustomCurve(t, tmp.GetCustomCurve(t));
            }
        }

        return src;
    }
}

}