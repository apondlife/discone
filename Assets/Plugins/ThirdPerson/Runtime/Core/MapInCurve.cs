using System;
using Soil;
using UnityEngine;

namespace ThirdPerson {

/// a normalized curve with a source range
[Serializable]
public struct MapInCurve {
    // -- fields --
    [Tooltip("the curve")]
    [SerializeField] AnimationCurve m_Curve;

    [Tooltip("the source range")]
    [SerializeField] FloatRange m_Src;

    // -- queries --
    /// evaluate the value along the curve
    public float Evaluate(float input) {
        var k = m_Src.InverseLerp(input);

        if (m_Curve != null && m_Curve.length != 0) {
            k = m_Curve.Evaluate(k);
        }

        return k;
    }

    // -- debug --
    public override string ToString() {
        return $"<MapInCurve src={m_Src}>";
    }
}

}