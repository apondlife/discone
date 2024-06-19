using System;
using UnityEngine;

namespace Soil {

/// a custom foldout attribute that doesn't complain about data type
[AttributeUsage(AttributeTargets.Field)]
public sealed class FoldoutAttribute: PropertyAttribute {
    // -- props --
    /// .
    public readonly string Name;

    // -- lifetime --
    public FoldoutAttribute(string name) {
        Name = name;
    }
}

}