using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Soil {

/// a normalized curve with a max value
[Serializable]
[UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Soil", "Soil", "DurationCurve")]
public struct MaxInCurve: FloatTransform {
    // -- fields --
    [FormerlySerializedAs("m_Curve")]
    [Tooltip("the curve")]
    public AnimationCurve Curve;

    [FormerlySerializedAs("m_Duration")]
    [Tooltip("the source maximum")]
    public float Src;

    // -- FloatTransform --
    public float Evaluate(float input) {
        if (Src == 0f) {
            return 1f;
        }

        return MapCurve.Evaluate(Curve, input / Src);
    }

    // -- queries --
    /// the duration of the curve
    public float Duration {
        get => Src;
    }

    // -- debug --
    public override string ToString() {
        return $"<MaxInCurve Src={Src}>";
    }
}

}