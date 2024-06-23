using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Soil {

/// mathf "static" methods
public static class Mathx {
    // -- constants --
    public const float TAU = Mathf.PI * 2f;
    public const float PI_2 = Mathf.PI / 2f;
    public const float TINY = 1e-6f;
    public const float PHI = 1.6180339f;

    // -- queries --
    /// zeroes a float near zero
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Zero(float value, float min = TINY) {
        return IsZero(value, min) ? 0f : value;
    }

    /// zeroes a vector near zero in length
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Zero(Vector3 value, float min = TINY) {
        return IsZero(value, min) ? Vector3.zero : value;
    }

    /// if the float is near zero
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsZero(float value, float min = TINY) {
        return Math.Abs(value) < min;
    }

    /// if the vector is near zero
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsZero(Vector3 value, float min = TINY) {
        return IsZero(value.sqrMagnitude, min);
    }

    /// the square distance between the two points
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SqrDistance(Vector3 a, Vector3 b) {
        return Vector3.SqrMagnitude(b - a);
    }

    /// inverse lerp a value that may be outside the [0,1] range
    public static float InverseLerpUnclamped(float a, float b, float value) {
        return (value - a) / (b - a);
    }

    /// remap a value from one range to another
    public static float Remap(
        float min0, float max0,
        float min1, float max1,
        float value
    ) {
        return Mathf.Lerp(min1, max1, Mathf.InverseLerp(min0, max0, value));
    }

    /// integrate a vector smoothing out the derivative over time
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