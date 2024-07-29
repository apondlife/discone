using Soil;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {

// TODO: try to figure out a way to decay the trail when the particle stops
// TODO: may want to curve initial particle speed based on planar / surface speed
// TODO: the model is often lower than the bottom of the capsule, which causes the trail
// to clip through the model especially on short jumps

/// the character's speed lines effect
sealed class JumpTrail: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [FormerlySerializedAs("m_System")]
    [FormerlySerializedAs("m_TrailParticles")]
    [Tooltip("the trail particle system")]
    [SerializeField] ParticleSystem m_Particles;

    // -- props --
    /// the character container
    CharacterContainer c;

    /// a buffer for trail particles
    ParticleSystem.Particle[] m_Buffer;

    /// the eased position
    DynamicEase<Vector3> m_Position;

    /// if the particle is playing
    bool m_IsPlaying;

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
        var tuning = c.Tuning.Model.JumpTrail;

        var next = c.State.Next;

        // update existing particles to the eased position
        var count = m_Particles.GetParticles(m_Buffer);
        for (var i = 0; i < count; i++) {
            var particle = m_Buffer[i];
            // var burstParticle = m_BurstBuffer[i];

            // stop if the elapsed time exceeds follow duration or if falling
            var elapsed = particle.startLifetime - particle.remainingLifetime;
            if (elapsed >= particle.startLifetime) {
                continue;
            }

            // is this particle is still playing?
            var isPlaying = i == count - 1 && m_IsPlaying;
            if (isPlaying) {
                var shouldStop = (
                    elapsed > tuning.FollowDuration ||
                    next.Velocity.y <= Mathx.TINY ||
                    next.MainSurface.IsSome
                );

                if (shouldStop) {
                    isPlaying = false;
                    m_IsPlaying = false;
                }
            }

            // if not, zero the velocity (TODO: lerp?)
            if (!isPlaying) {
                particle.velocity = Vector3.zero;
                // burstParticle.velocity = Vector3.zero;
            }
            // otherwise, update this particle given current position
            else {
                m_Position.Update(delta, transform.position);

                var pos = m_Position.Value;
                var vel = m_Position.Velocity;

                particle.position = pos;
                particle.velocity = vel;

                // point in velocity direction
                // var rot = Quaternion.LookRotation(vel, next.Forward);
                var rot = Quaternion.LookRotation(vel);
                particle.rotation3D = rot.eulerAngles;

                Log.Temp.I($"particle rotation {particle.rotation}");
            }

            // update the particle
            m_Buffer[i] = particle;
        }

        m_Particles.SetParticles(m_Buffer, count);

        // emit new particles on jump
        if (next.Events.Contains(CharacterEvent.Jump)) {
            Emit(next);
        }
    }

    // -- commands --
    void Emit(CharacterState.Frame next) {
        var tuning = c.Tuning.Model.JumpTrail;

        // rotate emitter to oppose movement
        // transform.forward = -next.Direction;

        // TODO: get real jump velocity
        // update initial speed
        var dv = next.Velocity - c.State.Curr.Velocity;
        var jumpSpeed = dv.magnitude;

        // set initial speed
        var main = m_Particles.main;
        main.startLifetime = tuning.Lifetime;
        main.startSpeed = jumpSpeed;

        // point in jump direction
        var rot = Quaternion.LookRotation(-dv, next.Forward);

        // shape/particle needs euler in degrees/radians respectively
        var euler = rot.eulerAngles;

        // rotate the emission shapes
        var shape = m_Particles.shape;
        shape.rotation = euler;

        // makes the particle oriented towards +z, instead of the -z default
        main.flipRotation = 1f;

        // update the start rotation
        euler *= Mathf.Deg2Rad;
        main.startRotationX = euler.x;
        main.startRotationY = euler.y;
        main.startRotationZ = euler.z;

        // emit one particle
        m_IsPlaying = true;
        m_Particles.Emit(1);

        // initialize the position ease w/ the initial particle pos and start speed
        var count = m_Particles.GetParticles(m_Buffer);
        if (count <= 0) {
            Log.Model.E($"no initial particle for jump trail");
            return;
        }

        m_Position.Init(m_Buffer[count - 1].position, dv);
    }
}

}