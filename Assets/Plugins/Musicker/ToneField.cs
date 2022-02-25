using System;
using UnityEngine;

namespace Musicker {

/// a tone described with notation
[Serializable]
public sealed class ToneField {
    // -- fields --
    [Tooltip("the notated tone")]
    [SerializeField] string m_Notation;

    // -- props --
    /// the underlying tone
    Lazy<Tone> m_Tone;

    // -- lifetime --
    /// create a new tone field
    public ToneField() {
        m_Tone = new Lazy<Tone>(() => Notation.DecodeTone(m_Notation));
    }

    // -- queries --
    /// the underlying tone
    public Tone Value {
        get => m_Tone.Value;
    }
}

}