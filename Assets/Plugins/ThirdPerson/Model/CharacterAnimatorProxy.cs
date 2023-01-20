using System;
using UnityEngine;

namespace ThirdPerson {

public class CharacterAnimatorProxy: MonoBehaviour {
    // -- props --
    /// a proxied ik callback
    Action<int> m_OnAnimatorIk;

    // -- lifecyle --
    void OnAnimatorIK(int layerIndex) {
        m_OnAnimatorIk?.Invoke(layerIndex);
    }

    // -- commands --
    /// bind a proxied ik callback
    public void Bind(Action<int> onAnimatorIk) {
        m_OnAnimatorIk = onAnimatorIk;
    }
}

}