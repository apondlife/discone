using System;
using UnityEngine;

namespace ThirdPerson {

/// mathf "static" methods
public static class Mathx {
    /// integrate a vector smoothing out the derivative over time
    public static float InverseLerpUnclamped(float a, float b, float value) {
        return (value - a) / (b-a);
    }

    public static Vector3 Integrate_Heun<T>(
        Func<Vector3, T, Vector3> derivative,
        Vector3 v0,
        float dt,
        in T args
    ) {
        // calculate current derivative
        var a0 = derivative(v0, args);

        // extrapolate from current derivative
        var v1 = v0 + a0 * dt;

        // heun's method, average current and next derivative for better prediction
        var a1 = derivative(v1, args);
        a0 = (a0 + a1) / 2.0f;

        // re-integrate the vector
        v1 = v0 + a0 * dt;

        return v1;
    }
}
}