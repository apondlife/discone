using UnityEngine;

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

    // -- props --
    /// the character container
    CharacterContainer c;

    /// the particle system
    ParticleSystem m_Particles;

    /// the character container
    ParticleSystem.Burst m_Burst;

    // -- lifecycle --
    void Start() {
        // set deps
        c = GetComponentInParent<Character>();

        m_Particles = GetComponent<ParticleSystem>();

        m_Burst = new ParticleSystem.Burst(0, 0);
    }

    void FixedUpdate() {
        // TODO: move into own script
        var next = c.State.Next;
        if (next.Events.Contains(CharacterEvent.Jump)) {
            var dv = next.Velocity - c.State.Curr.Velocity;
            m_Particles.transform.up = c.State.PerceivedSurface.Normal;

            var sqrSpeed = Vector3.SqrMagnitude(dv);
            // TODO: get actual jump speed
            var count = m_SqrSpeedToEmission.Evaluate(sqrSpeed);

            m_Burst.count = count;
            var main = m_Particles.main;
            main.startLifetimeMultiplier = m_SqrSpeedToLifetime.Evaluate(sqrSpeed);
            main.startSizeMultiplier = m_SqrSpeedToSize.Evaluate(sqrSpeed);

            m_Particles.emission.SetBurst(0, m_Burst);

            m_Particles.Play();
        }
    }
}
}