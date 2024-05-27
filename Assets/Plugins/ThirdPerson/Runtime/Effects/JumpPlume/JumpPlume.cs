using Soil;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {
[RequireComponent(typeof(ParticleSystem))]
public class JumpPlume: MonoBehaviour {
    [Header("config")]
    [Tooltip("emission count as a fn o sqr speed delta")]
    [SerializeField] MapCurve m_SqrSpeedToEmission;

    [Tooltip("particle size as a fn o sqr speed delta")]
    [SerializeField] MapCurve m_SqrSpeedToSize;

    [Tooltip("lifetime as a fn o sqr speed delta")]
    [SerializeField] MapCurve m_SqrSpeedToLifetime;

    [FormerlySerializedAs("m_SqrSpeedToStartSpeed")]
    [Tooltip("start speed as a fn o sqr speed delta")]
    [SerializeField] MapCurve m_SqrSpeedToStartSpeedScale;

    // -- props --
    /// the character container
    CharacterContainer c;

    /// the particle system
    ParticleSystem m_Particles;

    /// the particle system start speed range
    ParticleSystem.MinMaxCurve m_StartSpeed;

    // -- lifecycle --
    void Start() {
        // set deps
        c = GetComponentInParent<CharacterContainer>();

        m_Particles = GetComponent<ParticleSystem>();
        m_StartSpeed = m_Particles.main.startSpeed;
    }

    void FixedUpdate() {
        // TODO: move into own script
        var next = c.State.Next;
        if (next.Events.Contains(CharacterEvent.Jump)) {
            var dv = next.Velocity - c.State.Curr.Velocity;
            m_Particles.transform.up = c.State.Next.PerceivedSurface.Normal;

            // TODO: get actual jump speed
            var sqrSpeed = Vector3.SqrMagnitude(dv);
            var count = (int)m_SqrSpeedToEmission.Evaluate(sqrSpeed);

            var main = m_Particles.main;
            main.startLifetime = m_SqrSpeedToLifetime.Evaluate(sqrSpeed);
            main.startSize = m_SqrSpeedToSize.Evaluate(sqrSpeed);

            var startSpeedScale = m_SqrSpeedToStartSpeedScale.Evaluate(sqrSpeed);
            var startSpeed = m_StartSpeed;
            startSpeed.constantMin *= startSpeedScale;
            startSpeed.constantMax *= startSpeedScale;
            main.startSpeed = startSpeed;

            m_Particles.Emit(count);
        }
    }
}
}