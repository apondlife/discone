using System;
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
    [FormerlySerializedAs("m_Particles")]
    [FormerlySerializedAs("m_System")]
    [Tooltip("the trail particle system")]
    [SerializeField] ParticleSystem m_TrailParticles;

    [Tooltip("the burst particle system")]
    [SerializeField] ParticleSystem m_BurstParticles;

    // -- props --
    /// the character container
    CharacterContainer c;

    /// a buffer for trail particles
    ParticleSystem.Particle[] m_TrailBuffer;

    /// a buffer for burst particles
    ParticleSystem.Particle[] m_BurstBuffer;

    /// the eased position
    DynamicEase<Vector3> m_Position;

    /// if the particle is playing
    bool m_IsPlaying;

    // -- lifecycle --
    void Awake() {
        // set deps
        c = GetComponentInParent<CharacterContainer>();

        // allocate a buffer for the trail particles
        m_TrailBuffer = new ParticleSystem.Particle[m_TrailParticles.main.maxParticles];
        m_BurstBuffer = new ParticleSystem.Particle[m_BurstParticles.main.maxParticles];

        // capture the offset relative to the model
        m_Position = new DynamicEase<Vector3>(c.Tuning.Model.JumpTrail.Position);
    }

    void FixedUpdate() {
        var delta = Time.deltaTime;
        var tuning = c.Tuning.Model.JumpTrail;

        var next = c.State.Next;

        // update existing particles to the eased position
        var trailCount = m_TrailParticles.GetParticles(m_TrailBuffer);
        var burstCount = m_BurstParticles.GetParticles(m_BurstBuffer);

        for (var i = 0; i < trailCount; i++) {
            var trailParticle = m_TrailBuffer[i];
            var burstParticle = m_BurstBuffer[i];

            // stop if the elapsed time exceeds follow duration or if falling
            var elapsed = trailParticle.startLifetime - trailParticle.remainingLifetime;
            if (elapsed >= trailParticle.startLifetime) {
                continue;
            }

            // is this particle is still playing?
            var isPlaying = i == trailCount - 1 && m_IsPlaying;
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
                trailParticle.velocity = Vector3.zero;
                // burstParticle.velocity = Vector3.zero;
            }
            // otherwise, update this particle given current position
            else {
                m_Position.Update(delta, transform.position);

                var pos = m_Position.Value;
                var vel = m_Position.Velocity;

                trailParticle.position = pos;
                trailParticle.velocity = vel;

                // Log.Temp.I($"bp {burstParticle.startLifetime}");
                // if (burstParticle.remainingLifetime > 0f) {
                //     burstParticle.position = pos;
                //     burstParticle.velocity = vel;
                // }
            }

            // update the particle
            m_TrailBuffer[i] = trailParticle;
            m_BurstBuffer[i] = burstParticle;
        }

        m_TrailParticles.SetParticles(m_TrailBuffer, trailCount);
        m_BurstParticles.SetParticles(m_BurstBuffer, burstCount);

        // emit new particles on jump
        if (next.Events.Contains(CharacterEvent.Jump)) {
            Emit(next);
        }
    }

    // -- commands --
    void Emit(CharacterState.Frame next) {
        var tuning = c.Tuning.Model.JumpTrail;

        // rotate emitter to oppose movement
        transform.forward = -next.Direction;

        // TODO: get real jump velocity
        // update initial speed
        var dv = next.Velocity - c.State.Curr.Velocity;
        var jumpSpeed = dv.magnitude;

        var trailMain = m_TrailParticles.main;
        trailMain.startLifetime = tuning.Lifetime;
        trailMain.startSpeed = jumpSpeed;

        // var burstMain = m_BurstParticles.main;
        // burstMain.startSpeed = jumpSpeed;

        // emit one particle
        m_IsPlaying = true;
        m_TrailParticles.Emit(1);

        // initialize the position ease w/ the initial particle pos and start speed
        var count = m_TrailParticles.GetParticles(m_TrailBuffer);
        if (count <= 0) {
            Log.Model.E($"no initial particle for jump trail");
            return;
        }

        m_Position.Init(m_TrailBuffer[count - 1].position, dv);
    }
}

}