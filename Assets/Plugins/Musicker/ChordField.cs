using System;
using UnityEngine;

namespace Musicker {

/// a chord described with notation
[Serializable]
public sealed class ChordField {
    // -- fields --
    [Tooltip("the notated chord")]
    [SerializeField] string m_Notation;

    // -- props --
    /// the underlying chord
    Lazy<Chord> m_Chord;

    // -- lifetime --
    /// create a new chord field
    public ChordField() {
        m_Chord = new Lazy<Chord>(() => Notation.DecodeChord(m_Notation));
    }

    // -- queries --
    /// the underlying chord
    public Chord Value {
        get => m_Chord.Value;
    }
}

}