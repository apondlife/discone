using System;
using UnityEngine;

namespace Soil {

/// a dynamic pid controller for a moving target value
/// see: https://www.youtube.com/watch?v=KPoeNZZ6H4s
[Serializable]
public struct DynamicEasing {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the frequency")]
    public float F;

    [Tooltip("the damping")]
    public float Z;

    [Tooltip("the responsiveness")]
    public float R;

    // -- props --
    /// the previous target value
    Vector3 m_Target;

    /// the current position
    Vector3 m_Pos;

    /// the current velocity
    Vector3 m_Velocity;

    /// difficult math to describe, watch the video
    float m_K1, m_K2, m_K3;

    // -- commands --
    /// setup with an initial value
    public void Init(Vector3 initial) {
        // compute terms
        var pif1 = Mathf.PI * F;
        var pif2 = pif1 * 2f;

        m_K1 = Z / pif1;
        m_K2 = 1f / (pif2 * pif2);
        m_K3 = R * Z / pif2;

        // initialize state
        m_Target = initial;
        m_Pos = initial;
        m_Velocity = Vector3.zero;
    }

    /// move towards the target with an estimated target velocity
    public void Update(float delta, Vector3 target) {
        var targetVelocity = (target - m_Target) / delta;
        Update(delta, m_Target, targetVelocity);
    }

    /// move towards the target
    public void Update(float delta, Vector3 target, Vector3 targetVelocity) {
        m_Target = target;

        var k2 = Mathf.Max(m_K2, 1.1f * (delta * delta / 4f + delta * m_K1 / 2f));

        var pos = m_Pos;
        var velocity = m_Velocity;

        pos += delta * velocity;
        velocity += delta * (target + m_K3 * targetVelocity - pos - m_K1 * velocity) / k2;

        m_Pos = pos;
        m_Velocity = velocity;
    }

    // -- queries --
    /// the current target
    public Vector3 Target {
        get => m_Target;
    }

    /// the current position
    public Vector3 Pos {
        get => m_Pos;
    }
}

}