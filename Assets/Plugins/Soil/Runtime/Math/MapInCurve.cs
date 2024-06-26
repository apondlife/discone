using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Soil {

/// a normalized curve with a source range
[UnityEngine.Scripting.APIUpdating.MovedFrom(true, "ThirdPerson", "ThirdPerson", "MapInCurve")]
[Serializable]
public struct MapInCurve: FloatTransform {
    // -- fields --
    [FormerlySerializedAs("m_Curve")]
    [Tooltip("the curve")]
    public AnimationCurve Curve;

    [FormerlySerializedAs("m_Src")]
    [Tooltip("the source range")]
    public FloatRange Src;

    // -- FloatTransform --
    public float Evaluate(float input) {
        var k = Src.InverseLerp(input);

        if (Curve != null && Curve.length != 0) {
            k = Curve.Evaluate(k);
        }

        return k;
    }

    // -- debug --
    public override string ToString() {
        return $"<MapInCurve src={Src}>";
    }
}

}