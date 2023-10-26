using System;
using UnityEngine;

namespace ThirdPerson {

/// mathf "static" methods
public static class Mathx {
    // -- constants --
    public const float PI_2 = Mathf.PI / 2f;
    public const float SMOL = 1e-6f;

    // -- queries --
    /// integrate a vector smoothing out the derivative over time
    public static float InverseLerpUnclamped(float a, float b, float value) {
        return (value - a) / (b-a);
    }

    /// zeroes a float near epsilon
    public static float Zero(float value, float min = SMOL) {
        return Math.Abs(value) < min ? 0f : value;
    }

    /// zeroes a vector near epsilon
    public static Vector3 Zero(Vector3 value, float min = SMOL) {
        return value.magnitude < min ? Vector3.zero : value;
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