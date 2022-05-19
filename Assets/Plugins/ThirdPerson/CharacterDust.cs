using UnityEngine;

namespace ThirdPerson {

/// the character's dust effect
public class CharacterDust: MonoBehaviour {
    // -- tunables --
    [Header("tunables")]
    [Tooltip("the floor particle emission per unit of speed")]
    [SerializeField] float m_FloorParticlesBaseEmission;

    // -- refs --
    [Header("refs")]
    [Tooltip("the wall particle emitter")]
    [SerializeField] ParticleSystem m_WallParticles;

    [Tooltip("the floor particle emitter")]
    [SerializeField] ParticleSystem m_FloorParticles;

    [Tooltip("the jump particle emitter")]
    [SerializeField] ParticleSystem m_JumpParticles;

    [Tooltip("the particle that shows horizontal speed")]
    [SerializeField] ParticleSystem m_SpeedLine;

    // -- props --
    /// the character's state
    CharacterState m_State;

    bool isDebug;

    // -- lifecycle --
    void Start() {
        var character = GetComponentInParent<Character>();
        m_State = character.State;

        isDebug = character.name == "icecream.1";
    }

    void FixedUpdate() {
        if (m_WallParticles.isPlaying && !m_State.IsOnWall) {
            m_WallParticles.Stop();
        }

        if (m_State.IsOnWall) {
            if (!m_WallParticles.isPlaying) {
                m_WallParticles.Play();
            }

            if (!m_State.Collision.IsNone) {
                var c = m_State.Collision;
                var t = m_WallParticles.transform;
                t.position = c.Point;
                t.forward = -c.Normal;
            }
        }

        if (m_FloorParticles.isPlaying && !m_State.IsGrounded) {
            m_FloorParticles.Stop();
        }

        if (m_State.IsGrounded) {
            if (!m_FloorParticles.isPlaying) {
                m_FloorParticles.Play();
            }

            if (!m_State.Collision.IsNone) {
                var t = m_FloorParticles.transform;
                var c = m_State.Collision;
                t.position = c.Point;
            }

            var emission = m_FloorParticles.emission;
            var emissionRate = m_FloorParticlesBaseEmission * m_State.PlanarVelocity.sqrMagnitude;
            emission.rateOverTimeMultiplier = emissionRate;
        }

        if (!m_State.GetFrame(1).IsGrounded && m_State.GetFrame(0).IsGrounded) {
            if(!m_JumpParticles.isPlaying) {
                m_JumpParticles.Play();
            }
        }


        UpdateSpeedLine();
    }

    void UpdateSpeedLine() {
        var main = m_SpeedLine.main;
        main.startSpeed = m_State.PlanarVelocity.magnitude;
        m_SpeedLine.transform.forward = -m_State.PlanarVelocity.normalized;
    }
}
}