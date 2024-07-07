using System;
using UnityEngine;

namespace Soil {

/// a normalized curve with a min & max value
[Serializable]
public struct MapOutCurve: FloatTransform {
    // -- fields --
    [Tooltip("the curve")]
    public AnimationCurve Curve;

    [Tooltip("the destination range")]
    public FloatRange Dst;

    // -- FloatTransform --
    public float Evaluate(float input) {
        return MapCurve.Evaluate(Curve, Dst, input);
    }

    // -- debug --
    public override string ToString() {
        return $"<MapOutCurve dst={Dst}>";
    }
}

}