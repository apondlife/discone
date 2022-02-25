using System;
using UnityEngine;

namespace Musicker {

/// a progression described with notation
[Serializable]
public sealed class ProgressionField {
    // -- fields --
    [Tooltip("the notated progression")]
    [SerializeField] string m_Notation;

    // -- props --
    /// the underlying progression
    Lazy<Progression> m_Progression;

    // -- lifetime --
    /// create a new progression field
    public ProgressionField() {
        m_Progression = new Lazy<Progression>(() => Notation.DecodeProgression(m_Notation));
    }

    // -- queries --
    /// the underlying progression
    public Progression Value {
        get => m_Progression.Value;
    }
}

}