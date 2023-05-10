using UnityEngine;

namespace ThirdPerson {

/// the character's grounded movement smoke effects
public sealed class CharacterMoveSmoke: MonoBehaviour {
    // -- types --
    // the smoke source
    enum Source {
        None,
        Dash,
        Stop,
    }

    // -- tuning --
    [Header("tuning")]
    [Tooltip("the magnitude of a stop's acceleration")]
    [SerializeField] float m_StopAcceleration;

    // -- refs --
    [Header("refs")]
    [Tooltip("the floor dust particle emitter (high speed)")]
    [SerializeField] ParticleSystem m_Particles;

    // -- props --
    /// the character's state
    CharacterState m_State;

    /// the character's input
    CharacterInput m_Input;

    /// if the stop smoke can be played
    bool m_IsStopSmokeReady;

    // -- lifecycle --
    void Start() {
        var character = GetComponentInParent<Character>();

        // set deps
        m_State = character.State;
        m_Input = character.Input;
    }

    void FixedUpdate() {
        // if we leave the ground, reset
        if (!m_State.Next.IsOnGround) {
            m_IsStopSmokeReady = true;
            return;
        }

        var source = Source.None;

        // play smoke when we start moving
        if (source == Source.None) {
            if (m_State.WasStopped && !m_State.IsStopped) {
                source = Source.Dash;
            }
        }

        // play smoke when we're stopping
        if (source == Source.None && m_IsStopSmokeReady) {
            var acceleration = Vector3.ProjectOnPlane(
                m_State.Curr.Acceleration,
                m_State.Ground.Normal
            );

            var accelerationDotVelocity = Vector3.Dot(
                acceleration,
                m_State.Velocity.normalized
            );

            if (accelerationDotVelocity < m_StopAcceleration) {
                source = Source.Stop;
            }
        }

        // play smoke if we should
        if (source != Source.None) {
            m_Particles.Play();
            m_IsStopSmokeReady = source == Source.Dash;
        }
    }
}

}