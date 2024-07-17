using Soil;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {

/// the character's speed lines effect
sealed class JumpTrail: MonoBehaviour {
    // -- refs --
    [FormerlySerializedAs("m_System")]
    [Header("refs")]
    [Tooltip("the particle that shows horizontal speed")]
    [SerializeField] ParticleSystem m_Particles;

    // -- props --
    /// the character container
    CharacterContainer c;

    /// a buffer for particles
    ParticleSystem.Particle[] m_Buffer;

    /// the eased position
    DynamicEase<Vector3> m_Position;

    // -- lifecycle --
    void Awake() {
        // set deps
        c = GetComponentInParent<CharacterContainer>();

        // allocate a buffer for the trail particles
        m_Buffer = new ParticleSystem.Particle[m_Particles.main.maxParticles];

        // capture the offset relative to the model
        m_Position = new DynamicEase<Vector3>(c.Tuning.Model.JumpTrail.Position);
    }

    void FixedUpdate() {
        var delta = Time.deltaTime;

        // emit particles on jump
        var next = c.State.Next;
        if (next.Events.Contains(CharacterEvent.Jump)) {
            Emit(next);
        }

        // update existing particles to the eased position
        var count = m_Particles.GetParticles(m_Buffer, 1);
        Log.Temp.I($"got {count}");
        if (count > 0) {
            var i = count - 1;
            var particle = m_Buffer[i];

            // stop if the elapsed time exceeds follow duration or if falling
            var elapsed = particle.startLifetime - particle.remainingLifetime;
            var shouldStop = (
                elapsed > c.Tuning.Model.JumpTrail.FollowDuration ||
                next.Velocity.y <= 0f
            );

            Log.Temp.I($"should stop {elapsed} & {next.Velocity.y}");

            // if time to stop, stop the particle
            if (shouldStop) {
                particle.velocity = Vector3.zero;
            }
            // otherwise, ease towards the current position
            else {
                m_Position.Update(delta, transform.position);
                Log.Temp.I($"update pos to {m_Position.Value}");
                particle.position = m_Position.Value;
                particle.velocity = m_Position.Velocity;
            }

            m_Buffer[i] = particle;
            m_Particles.SetParticles(m_Buffer);
        }
    }

    // -- commands --
    void Emit(CharacterState.Frame next) {
        var dv = next.Velocity - c.State.Curr.Velocity;
        m_Particles.transform.up = c.State.Next.PerceivedSurface.Normal;

        // TODO: get actual jump speed
        var sqrSpeed = Vector3.SqrMagnitude(dv);

        // rotate emitter to oppose movement
        transform.forward = -next.Direction;

        var main = m_Particles.main;
        main.startLifetime = c.Tuning.Model.JumpTrail.Lifetime;
        main.startSpeed = dv.magnitude;

        // main.duration = sqrSpeed / 1000;
        m_Particles.Emit(1);

        // initialize the position ease from the initial particle pos
        var count = m_Particles.GetParticles(m_Buffer, 1);
        if (count <= 0) {
            Log.Model.E($"no initial particle for jump trail");
            return;
        }

        m_Position.Init(m_Buffer[count - 1].position, dv);
    }
}

}