using UnityEngine;

namespace Soil {

/// a named shader property (uniform)
public readonly struct ShaderProp {
    // -- props --
    /// the shader uniform id
    readonly int m_Id;

    // -- lifetime --
    ShaderProp(int id) {
        m_Id = id;
    }

    // -- commands --
    /// set the global float value
    public void Set(float value) {
        Shader.SetGlobalFloat(m_Id, value);
    }

    /// set the global color value
    public void Set(Color value) {
        Shader.SetGlobalColor(m_Id, value);
    }

    // -- conversions --
    /// convert the prop to an id
    public static implicit operator int(ShaderProp p) {
        return p.m_Id;
    }

    // -- factories --
    /// a shader prop for the given name
    public static ShaderProp Named(string name) {
        return new ShaderProp(Shader.PropertyToID(name));
    }
}

}