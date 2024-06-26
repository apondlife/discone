using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Soil {

/// a normalized curve with source and destination ranges
[UnityEngine.Scripting.APIUpdating.MovedFrom(true, "ThirdPerson", "ThirdPerson", "MapCurve")]
[Serializable]
public struct MapCurve: FloatTransform {
    // -- fields --
    [FormerlySerializedAs("m_Curve")]
    [Tooltip("the curve")]
    public AnimationCurve Curve;

    [FormerlySerializedAs("m_Src")]
    [Tooltip("the source range")]
    public FloatRange Src;

    [FormerlySerializedAs("m_Dst")]
    [Tooltip("the destination range")]
    public FloatRange Dst;

    // -- FloatTransform --
    public float Evaluate(float input) {
        var k = Src.InverseLerp(input);

        if (Curve != null && Curve.length != 0) {
            k = Curve.Evaluate(k);
        }

        return Dst.Lerp(k);
    }

    // -- debug --
    public override string ToString() {
        return $"<MapCurve src={Src} dst={Dst}>";
    }
}

}