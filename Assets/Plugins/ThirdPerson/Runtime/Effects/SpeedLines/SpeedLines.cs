using Soil;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {

/// the character's speed lines effect
sealed class SpeedLines: CharacterEffect {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the scale for start speed as a fn of sqr velocity")]
    [SerializeField] float m_SpeedScale;

    [Tooltip("the scale for rotation over time as a fn of acceleration")]
    [SerializeField] Vector2 m_RotationScale;

    [Tooltip("the max speed change per second")]
    [SerializeField] float m_MaxSpeedDelta;

    [Tooltip("the max orbit speed change per second")]
    [SerializeField] float m_MaxOrbitDelta;

    [Tooltip("the max rotation change per second")]
    [SerializeField] float m_MaxRotationDelta;

    [Tooltip("the lifetime multiplier as a fn of direction change")]
    [SerializeField] MapOutCurve m_DirectionScale;

    // -- refs --
    [Header("refs")]
    [Tooltip("the anchor transform")]
    [SerializeField] Transform m_Anchor;

    [FormerlySerializedAs("m_System")]
    [Tooltip("the particle that shows horizontal speed")]
    [SerializeField] ParticleSystem m_Particles;

    // -- props --
    /// a buffer for particles
    NativeArray<ParticleSystem.Particle> m_Buffer;

    // -- lifecycle --
    protected override void Awake() {
        base.Awake();

        // set props
        m_Buffer = new NativeArray<ParticleSystem.Particle>(m_Particles.main.maxParticles, Allocator.Persistent);
    }

    void FixedUpdate() {
        // attach to the anchor
        if (m_Anchor) {
            transform.position = m_Anchor.position;
        }

        var v = c.State.Next.SurfaceVelocity;
        var dir = v.normalized;

        // scale speed line based on ground speed
        var main = m_Particles.main;

        var destSpeed = v.sqrMagnitude * m_SpeedScale;
        var nextSpeed = Mathf.MoveTowards(
            main.startSpeed.constant,
            destSpeed,
            m_MaxSpeedDelta * Time.deltaTime
        );

        main.startSpeed = nextSpeed;

        // rotate emitter to oppose movement
        if (v != Vector3.zero) {
            transform.forward = Vector3.RotateTowards(
                transform.forward,
                dir,
                m_MaxRotationDelta * Time.deltaTime,
                0.0f
            );
        }

        // rotate lines as character accelerates
        var vol = m_Particles.velocityOverLifetime;

        var a = transform.InverseTransformVector(c.State.Curr.Acceleration);
        var destOrbital = new Vector2(a.y, -a.x) * m_RotationScale * m_SpeedScale;
        var nextOrbital = Vector2.MoveTowards(
            new Vector2(
                vol.orbitalX.constant,
                vol.orbitalY.constant
            ),
            destOrbital,
            m_MaxOrbitDelta * Time.deltaTime
        );

        vol.orbitalX = nextOrbital.x;
        vol.orbitalY = nextOrbital.y;
        vol.orbitalZ = 1f;

        // update existing particles
        var count = m_Particles.GetParticles(m_Buffer);
        for (var i = 0; i < count; i++) {
            var particle = m_Buffer[i];

            // reduce the lifetime of particles on direction change
            var dirAlignment = Mathf.Max(0f, -Vector3.Dot(
                dir,
                particle.totalVelocity.normalized
            ));

            var dirScale = m_DirectionScale.Evaluate(dirAlignment);
            particle.remainingLifetime *= dirScale;

            m_Buffer[i] = particle;
        }

        m_Particles.SetParticles(m_Buffer, count);
    }

    void OnDestroy() {
        m_Buffer.Dispose();
    }
}

}