using System;
using UnityEngine;

namespace Soil {

/// a normalized curve with source and destination ranges
[UnityEngine.Scripting.APIUpdating.MovedFrom(true, "ThirdPerson", "ThirdPerson", "MapCurve")]
[Serializable]
public struct MapCurve {
    // -- fields --
    [Tooltip("the curve")]
    [SerializeField] AnimationCurve m_Curve;

    [Tooltip("the source range")]
    [SerializeField] FloatRange m_Src;

    [Tooltip("the destination range")]
    [SerializeField] FloatRange m_Dst;

    // -- queries --
    /// evaluate the value along the curve
    public float Evaluate(float input) {
        var k = m_Src.InverseLerp(input);

        if (m_Curve != null && m_Curve.length != 0) {
            k = m_Curve.Evaluate(k);
        }

        return m_Dst.Lerp(k);
    }

    // -- debug --
    public override string ToString() {
        return $"<MapCurve src={m_Src} dst={m_Dst}>";
    }
}

}