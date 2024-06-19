using UnityEngine;

namespace Soil {

/// a named shader property (uniform)
public readonly struct ShaderProp {
    // -- props --
    /// the shader uniform id
    readonly int m_Id;

    // -- lifetime --
    public ShaderProp(int id) {
        m_Id = id;
    }

    /// a shader prop for the given name
    public ShaderProp(string name, string prefix = "_") {
        m_Id = Shader.PropertyToID(prefix + name);
    }

    // -- commands --
    /// set the global float value
    public void Set(float value) {
        Shader.SetGlobalFloat(m_Id, value);
    }

    /// set the global color value
    public void Set(UnityEngine.Color value) {
        Shader.SetGlobalColor(m_Id, value);
    }

    // -- conversions --
    /// convert the prop to an id
    public static implicit operator int(ShaderProp p) {
        return p.m_Id;
    }
}

}