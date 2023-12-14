using System;
using UnityEngine;

namespace Soil {

/// a custom range attribute that doesn't complain about data type
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public sealed class RangeAttribute: PropertyAttribute {
    // -- props --
    /// .
    public readonly float Min;

    /// .
    public readonly float Max;

    // -- lifetime --
    public RangeAttribute(float min, float max) {
        Min = min;
        Max = max;
    }
}

}