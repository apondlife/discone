using UnityEngine;

namespace Soil {

/// a box for an animation curve
struct AnimationCurveBox: FloatTransform {
    // -- fields --
    [Tooltip("the inner animation curve")]
    public AnimationCurve Inner;

    // -- FloatTransform --
    public float Evaluate(float value) {
        return Inner.Evaluate(value);
    }
}

}