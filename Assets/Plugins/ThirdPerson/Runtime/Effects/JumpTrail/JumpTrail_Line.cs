using System.Timers;
using Soil;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {

// TODO: try to figure out a way to decay the trail when the particle stops
// TODO: may want to curve initial particle speed based on planar / surface speed
// TODO: the model is often lower than the bottom of the capsule, which causes the trail
// to clip through the model especially on short jumps

/// the character's speed lines effect
class JumpTrail_Line: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip(".")]
    [SerializeField] TrailRenderer m_Renderer;

    // -- props --
    /// the character container
    CharacterContainer c;

    /// the eased position
    DynamicEase<Vector3> m_Position;

    /// if the particle is playing
    bool m_IsEmitting;

    /// the lifetime of the trail
    EaseTimer m_Lifetime;

    // -- lifecycle --
    void Awake() {
        // set deps
        c = GetComponentInParent<CharacterContainer>();

        // capture the offset relative to the model
        var tuning = c.Tuning.Model.JumpTrail;
        m_Position = new DynamicEase<Vector3>(tuning.Position);
        m_Lifetime = new EaseTimer(new(tuning.Lifetime));
    }

    void FixedUpdate() {
        var delta = Time.deltaTime;
        var tuning = c.Tuning.Model.JumpTrail;

        var next = c.State.Next;

        // update existing particles to the eased position
        // var count = m_Particles.GetParticles(m_Buffer);
        if (m_Lifetime.TryTick()) {
        // for (var i = 0; i < count; i++) {
            // var particle = m_Buffer[i];
            // var burstParticle = m_BurstBuffer[i];

            // is this particle is still playing?
            // var isPlaying = i == count - 1 && m_IsEmitting;
            var isPlaying = m_Renderer.emitting;
            // m_Renderer.time = m_Lifetime.Duration - m_Lifetime.Elapsed;
            if (isPlaying) {
                var elapsed = m_Lifetime.Elapsed;
                Log.Temp.I($"elapsed {elapsed} v {tuning.FollowDuration}");
                var shouldStop = (
                    elapsed > tuning.FollowDuration ||
                    next.Velocity.y <= Mathx.TINY ||
                    next.MainSurface.IsSome
                );

                if (shouldStop) {
                    isPlaying = false;
                    // m_Renderer.emitting = false;
                }
            }

            // if not, zero the velocity (TODO: lerp?)
            if (!isPlaying) {
                m_Renderer.emitting = false;
            }
            // otherwise, update this particle given current position
            else {
                m_Position.Update(delta, transform.position);

                var pos = m_Position.Value;
                var vel = m_Position.Velocity;
                var trs = m_Renderer.transform;

                trs.position = pos;
                // particle.velocity = vel;

                // point in velocity direction
                // var rot = Quaternion.LookRotation(vel, next.Forward);
                // var rot = Quaternion.LookRotation(vel);
                // particle.rotation3D = rot.eulerAngles;

                // Log.Temp.I($"particle rotation {particle.rotation}");
            }
        //
        //     // update the particle
        //     m_Buffer[i] = particle;
        }

        // m_Particles.SetParticles(m_Buffer, count);

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
        // var jumpSpeed = dv.magnitude;

        // set initial speed
        // var main = m_Particles.main;
        // main.startLifetime = tuning.Lifetime;
        // main.startSpeed = jumpSpeed;
        //
        // // point in jump direction
        // var rot = Quaternion.LookRotation(-dv, next.Forward);
        //
        // // shape/particle needs euler in degrees/radians respectively
        // var euler = rot.eulerAngles;
        //
        // // rotate the emission shapes
        // var shape = m_Particles.shape;
        // shape.rotation = euler;
        //
        // // makes the particle oriented towards +z, instead of the -z default
        // main.flipRotation = 1f;
        //
        // // update the start rotation
        // euler *= Mathf.Deg2Rad;
        // main.startRotationX = euler.x;
        // main.startRotationY = euler.y;
        // main.startRotationZ = euler.z;

        // emit one particle
        // m_IsEmitting = true;
        var pos = transform.position;
        m_Renderer.transform.position = pos;
        m_Renderer.emitting = true;

        m_Lifetime.Play();

        // initialize the position ease w/ the initial particle pos and start speed
        m_Position.Init(pos, dv);
    }
}

}