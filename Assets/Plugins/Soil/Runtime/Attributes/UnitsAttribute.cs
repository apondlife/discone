using System;
using UnityEngine;

namespace Soil {

/// the location to place the unit
[Flags]
public enum UnitsFor {
    Src = 1 << 0,
    Dst = 1 << 1
}

/// labels a field with a unit
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class UnitsAttribute: PropertyAttribute {
    // -- props --
    /// the label for the unit
    public readonly string Label;

    /// the location of the unit
    public readonly UnitsFor For;

    // -- lifetime --
    public UnitsAttribute(
        string label,
        UnitsFor @for = UnitsFor.Src | UnitsFor.Dst
    ) {
        Label = label;
        For = @for;
    }
}

}