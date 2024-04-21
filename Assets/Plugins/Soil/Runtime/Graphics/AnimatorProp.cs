using UnityEngine;

namespace Soil {

/// a named animator property
public readonly struct AnimatorProp {
    // -- props --
    /// the shader uniform id
    readonly int m_Id;

    // -- lifetime --
    public AnimatorProp(int id) {
        m_Id = id;
    }

    public AnimatorProp(string name) {
        m_Id = Animator.StringToHash(name);
    }

    // -- conversions --
    /// convert the prop to an id
    public static implicit operator int(AnimatorProp p) {
        return p.m_Id;
    }
}

}