using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThirdPerson {

public class CharacterAnimatorProxy : MonoBehaviour
{
    Action<int> m_OnAnimatorIk;

    // -- lifecyle --
    void OnAnimatorIK(int layerIndex) {
        m_OnAnimatorIk?.Invoke(layerIndex);
    }

    // -- commands --
    public void Bind(Action<int> onAnimatorIk) {
        m_OnAnimatorIk = onAnimatorIk;
    }
}
}