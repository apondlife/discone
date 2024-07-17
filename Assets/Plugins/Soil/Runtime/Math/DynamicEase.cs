using System;
using UnityEngine;

namespace Soil {

// TODO: consider some math optimizations: https://stackoverflow.com/questions/53785910/avoiding-the-overhead-of-c-sharp-virtual-calls
// everything becomes structs, arithmetic explicitly passed in, this type no serializable

/// a dynamic pid controller for a moving target value
/// see: https://www.youtube.com/watch?v=KPoeNZZ6H4s
[Serializable]
public sealed class DynamicEase<T>: DynamicEase {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the ease configuration")]
    [SerializeField] DynamicEase.Config m_Config;

    #pragma warning disable CS0414
    [Tooltip("the configuration source")]
    [SerializeField] DynamicEase.ConfigSource m_ConfigSource;
    #pragma warning restore CS0414

    [Tooltip("if the ease is disabled")]
    [SerializeField] bool m_IsDisabled;

    // -- deps --
    /// the arithmetic operators for the parameterized type
    DynamicEase.Arithmetic<T> a;

    // -- props --
    /// the previous target value
    T m_Target;

    /// the current position
    T m_Value;

    /// the current velocity
    T m_Velocity;

    // -- lifetime --
    /// create a new dynamic ease w/ a config
    public DynamicEase(DynamicEase.Config config) {
        // set deps (placeholder until Init)
        a = null;

        // set config
        m_Config = config;
        m_ConfigSource = DynamicEase.ConfigSource.Local;

        // set defaults
        m_IsDisabled = false;
        m_Target = default;
        m_Value = default;
        m_Velocity = default;
    }

    // -- commands --
    /// setup with an initial value
    public void Init(T initial, T initialVelocity = default) {
        // set deps
        a = DynamicEase.Arithmetic<T>.Instance;

        // set props
        m_Target = initial;
        m_Value = initial;
        m_Velocity = initialVelocity;
    }

    /// move towards the target with an estimated target velocity
    public void Update(
        float delta,
        T target
    ) {
        // estimate target velocity:
        //   targetVelocity = (target - m_Target) / delta
        var targetVelocity = a.Div(a.Sub(target, m_Target), delta);
        Update(delta, target, targetVelocity: targetVelocity);
    }

    /// move towards the target
    public void Update(
        float delta,
        T target,
        T targetVelocity
    ) {
        m_Target = target;
        if (m_IsDisabled) {
            m_Value = target;
            return;
        }

        var pos = m_Value;
        var velocity = m_Velocity;

        // integrate position by current velocity
        pos = a.Add(pos, a.Mul(velocity, delta));

        // clamp k2 to guarantee stability
        var k1 = m_Config.K1;
        var k2 = Mathf.Max(m_Config.K2, 1.1f * (delta * delta / 4 + delta * m_Config.K1 / 2));
        var k3 = m_Config.K3;

        // integrate velocity by weird fzr acceleration:
        //   velocity += delta / k2_stabilized * (target + targetVelocity * k3 - pos - velocity * k1);
        var aK1 = a.Mul(velocity, k1);
        var aK2 = a.Mul(targetVelocity, k3);
        var acc = a.Div(a.Sub(a.Sub(a.Add(target, aK2), pos), aK1), k2);
        velocity = a.Add(velocity, a.Mul(acc, delta));

        m_Value = pos;
        m_Velocity = velocity;
    }

    // -- queries --
    /// the current target
    public T Target {
        get => m_Target;
    }

    /// the current value
    public T Value {
        get => m_Value;
    }

    /// the current value
    public T Velocity {
        get => m_Velocity;
    }
}

public interface DynamicEase {
    // -- config --
    /// the config source for the ease
    public enum ConfigSource {
        Local,
        External
    }

    /// the config for the ease
    [Serializable]
    public sealed class Config {
        [Tooltip("the frequency")]
        public float F;

        [Tooltip("the damping")]
        public float Z;

        [Tooltip("the responsiveness")]
        public float R;

        /// difficult math to describe, watch the video
        [HideInInspector]
        public float K1, K2, K3;

        // -- lifetime --
        /// compute the eigenvalue (or w/e) terms given f, z, r
        public Config(float f, float z, float r) {
            F = f;
            Z = z;
            R = r;

            if (f == 0f) {
                K1 = 0f;
                K2 = 0f;
                K3 = 0f;
            } else {
                var pif1 = Mathf.PI * f;
                var pif2 = pif1 * 2f;
                K1 = z / pif1;
                K2 = 1f / (pif2 * pif2);
                K3 = r * z / pif2;
            }
        }
    }

    // -- type support --
    /// provides arithmetic operations for a generic value type
    interface Arithmetic<T> {
        /// add the right-hand operand to the left
        T Add(T lhs, T rhs);

        /// subtract the right-hand operand from the left
        T Sub(T lhs, T rhs);

        /// multiply the left-hand operand by a scalar
        T Mul(T lhs, float rhs);

        /// divide the left-hand operand by a scalar
        T Div(T lhs, float rhs);

        /// get the instance for type T
        static Arithmetic<T> Instance {
            get => typeof(T) switch {
                { } t when t == typeof(float)   => (Arithmetic<T>)FloatArithmetic.Instance,
                { } t when t == typeof(Vector3) => (Arithmetic<T>)VectorArithmetic.Instance,
                _ => throw new NotImplementedException($"Arithmetic<T> not implemented for type {typeof(T)}")
            };
        }
    }

    /// provides arithmetic operations for floats
    record FloatArithmetic: Arithmetic<float> {
        public float Add(float lhs, float rhs) => lhs + rhs;
        public float Sub(float lhs, float rhs) => lhs - rhs;
        public float Mul(float lhs, float rhs) => lhs * rhs;
        public float Div(float lhs, float rhs) => lhs / rhs;

        public static FloatArithmetic Instance = new();
    }

    /// provides arithmetic operations for vectors
    record VectorArithmetic: Arithmetic<Vector3> {
        public Vector3 Add(Vector3 lhs, Vector3 rhs) => lhs + rhs;
        public Vector3 Sub(Vector3 lhs, Vector3 rhs) => lhs - rhs;
        public Vector3 Mul(Vector3 lhs, float rhs) => lhs * rhs;
        public Vector3 Div(Vector3 lhs, float rhs) => lhs / rhs;

        public static VectorArithmetic Instance = new();
    }

}

}