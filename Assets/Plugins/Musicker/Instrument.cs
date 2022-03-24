using UnityEngine;

namespace Musicker {

/// produces any note in the chromatic scale
[CreateAssetMenu(fileName = "Instrument", menuName = "Musicker/Instrument", order = 0)]
public class Instrument: ScriptableObject {
    // -- config --
    [Header("config")]
    [Tooltip("the chromatic scale (usually c3-based); must have a multiple of 12 notes")]
    [SerializeField] AudioClip[] m_Scale;

    // -- queries --
    /// the duration of any clip
    public float Duration {
        get => m_Scale[0].length;
    }

    /// find a random audio clip
    public AudioClip RandClip() {
        return m_Scale[Random.Range(0, Length)];
    }

    /// find the clip for a tone
    public AudioClip FindClip(Tone tone) {
        return m_Scale[tone.Steps % Length];
    }

    /// the length of the scale
    int Length {
        get => m_Scale.Length;
    }
}

}