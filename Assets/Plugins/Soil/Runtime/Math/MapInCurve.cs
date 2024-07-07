using System;
using UnityEngine;

namespace Soil {

/// a normalized curve with a source range
[Serializable]
public struct MapInCurve: FloatTransform {
    // -- fields --
    [Tooltip("the curve")]
    public AnimationCurve Curve;

    [Tooltip("the source range")]
    public FloatRange Src;

    // -- FloatTransform --
    public float Evaluate(float input) {
        return MapCurve.Evaluate(Curve, Src, input);
    }

    // -- debug --
    public override string ToString() {
        return $"<MapInCurve src={Src}>";
    }
}

}