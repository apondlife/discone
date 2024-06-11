using System;
using UnityEngine;

namespace Soil {

/// a dynamic pid controller for a moving target value
/// see: https://www.youtube.com/watch?v=KPoeNZZ6H4s
[Serializable]
public struct DynamicEase {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the frequency")]
    public float F;

    [Tooltip("the damping")]
    public float Z;

    [Tooltip("the responsiveness")]
    public float R;

    [Tooltip("if the ease curve is disabled")]
    [SerializeField] bool m_IsDisabled;

    /// difficult math to describe, watch the video
    [HideInInspector]
    [SerializeField] float m_K1, m_K2, m_K3;

    // -- props --
    /// the previous target value
    Vector3 m_Target;

    /// the current position
    Vector3 m_Value;

    /// the current velocity
    Vector3 m_Velocity;

    // -- commands --
    /// setup with an initial value
    public void Init(Vector3 initial) {
        m_Target = initial;
        m_Value = initial;
        m_Velocity = Vector3.zero;
    }

    /// move towards the target with an estimated target velocity
    public void Update(
        float delta,
        Vector3 target,
        bool isAlwaysEnabled = false
    ) {
        Update(delta, target, targetVelocity: (target - m_Target) / delta, isAlwaysEnabled);
    }

    /// move towards the target
    public void Update(
        float delta,
        Vector3 target,
        Vector3 targetVelocity,
        bool isAlwaysEnabled = false
    ) {
        m_Target = target;
        if (m_IsDisabled && !isAlwaysEnabled) {
            m_Value = target;
            return;
        }

        var k2 = Mathf.Max(m_K2, 1.1f * (delta * delta / 4f + delta * m_K1 / 2f));

        var pos = m_Value;
        var velocity = m_Velocity;

        pos += delta * velocity;
        velocity += delta * (target + m_K3 * targetVelocity - pos - m_K1 * velocity) / k2;

        m_Value = pos;
        m_Velocity = velocity;
    }

    // -- queries --
    /// the current target
    public Vector3 Target {
        get => m_Target;
    }

    /// the current value
    public Vector3 Value {
        get => m_Value;
    }

    /// compute the eigenvalue (or w/e) terms given f, z, r
    public static (float, float, float) ComputeTerms(float f, float z, float r) {
        var pif1 = Mathf.PI * f;
        var pif2 = pif1 * 2f;

        var k1 = z / pif1;
        var k2 = 1f / (pif2 * pif2);
        var k3 = r * z / pif2;

        return (k1, k2, k3);
    }

    // -- factories --
    public DynamicEase Clone() {
        return new DynamicEase() {
            F = F,
            Z = Z,
            R = R,
            m_K1 = m_K1,
            m_K2 = m_K2,
            m_K3 = m_K3,
            m_IsDisabled = m_IsDisabled,
        };
    }
}

}