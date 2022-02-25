using System;
using UnityEngine;

namespace Musicker {

/// a line described with notation
[Serializable]
public sealed class LineField {
    // -- fields --
    [Tooltip("the notated line")]
    [SerializeField] string m_Notation;

    // -- props --
    /// the underlying line
    Lazy<Line> m_Line;

    // -- lifetime --
    /// create a new line field
    public LineField() {
        m_Line = new Lazy<Line>(() => Notation.DecodeLine(m_Notation));
    }

    // -- queries --
    /// the underlying line
    public Line Value {
        get => m_Line.Value;
    }
}

}