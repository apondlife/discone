using System;
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

    /// if the particle is playing
    bool m_IsPlaying = false;

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
            }
            // otherwise, update this particle given current position
            else {
                // scale down lifetime if force opposes velocity in the horizontal plane
                var nextForce = next.PlanarForce;
                var nextForceMag = nextForce.magnitude;

                // if there's no force, we want to use
                var nextVelocity = nextForceMag == 0f ? next.Velocity : next.PlanarVelocity;
                var nextVelocityMag = nextVelocity.magnitude;

                var forceDotSpeed = 1f;
                if (nextForceMag == 0f && Vector3.Dot(next.Velocity.normalized, Vector3.up) == -1f) {
                    forceDotSpeed = -1f;
                    nextVelocityMag = next.Velocity.magnitude;
                }

                forceDotSpeed = Vector3.Dot(nextForce / nextForceMag, nextVelocity / nextVelocityMag);
                Log.Temp.I($"fds {forceDotSpeed} nfm {nextForce} nvm {nextVelocityMag}");
                if (forceDotSpeed <= 0f) {
                    forceDotSpeed = (1f + forceDotSpeed) * (nextForceMag + nextVelocityMag);

                    var totalLifetime = tuning.Lifetime * tuning.ForceDotSpeedToLifetimeScale.Evaluate(forceDotSpeed);
                    particle.remainingLifetime = totalLifetime - elapsed;
                    Log.Temp.I($"lifetime :: {forceDotSpeed} -> {totalLifetime} ({particle.startLifetime}) ... {particle.remainingLifetime}");
                }

                // ease towards current position
                m_Position.Update(delta, transform.position);

                particle.position = m_Position.Value;
                particle.velocity = m_Position.Velocity;
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

        var dv = next.Velocity - c.State.Curr.Velocity;
        m_Particles.transform.up = c.State.Next.PerceivedSurface.Normal;

        // rotate emitter to oppose movement
        transform.forward = -next.Direction;

        var main = m_Particles.main;
        main.startLifetime = tuning.Lifetime;
        main.startSpeed = dv.magnitude;

        m_IsPlaying = true;
        m_Particles.Emit(1);

        // initialize the position ease from the initial particle pos
        var count = m_Particles.GetParticles(m_Buffer);
        if (count <= 0) {
            Log.Model.E($"no initial particle for jump trail");
            return;
        }

        // initial velocity is impulse dir, unless we're jumping straight up, in which
        // case we don't want the trail to overshoot us (looks bad)
        m_Position.Init(m_Buffer[count - 1].position, dv);
    }
}

}