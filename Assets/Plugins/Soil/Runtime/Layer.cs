using System;
using UnityEngine;

namespace Soil {

/// a layer index
[Serializable]
public struct Layer {
    // -- props --
    /// the underlying index
    [HideInInspector]
    [SerializeField] int m_Index;

    // -- operators --
    /// convert a layer to an int
    public static implicit operator int(Layer layer) {
        return layer.m_Index;
    }

    /// convert an int to a layer
    public static implicit operator Layer(int index) {
        var layer = (Layer)default;
        layer.m_Index = index;
        return layer;
    }
}

}