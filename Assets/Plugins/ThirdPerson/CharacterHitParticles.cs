using UnityEngine;

namespace ThirdPerson {

/// the character's particle effects
public class CharacterHitParticles: MonoBehaviour {
    [Header("tunables")]
    [Tooltip("the floor particle emission per unit of speed")]
    [SerializeField] private float m_FloorParticlesBaseEmission;

    [Header("references")]
    [Tooltip("the wall particle emitter")]
    [SerializeField] private ParticleSystem m_WallParticles;

    [Tooltip("the floor particle emitter")]
    [SerializeField] private ParticleSystem m_FloorParticles;

    // -- props --
    /// the character's state
    CharacterState m_State;

    // -- lifecycle --
    void Start() {
        var character = GetComponentInParent<Character>();
        m_State = character.State;
    }

    void Update() {
        if(m_WallParticles.isPlaying && !m_State.IsOnWall) {
            m_WallParticles.Stop();
        }

        if(m_State.IsOnWall) {
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

        if(m_FloorParticles.isPlaying && !m_State.IsGrounded) {
            m_FloorParticles.Stop();
        }

        if(m_State.IsGrounded) {
            if (!m_FloorParticles.isPlaying) {
                m_FloorParticles.Play();
            }

            if (!m_State.Collision.IsNone) {
                var t = m_FloorParticles.transform;
                var c = m_State.Collision;
                t.position = c.Point;
            }

            var emission = m_FloorParticles.emission;
            var multiplier = m_State.PlanarVelocity.sqrMagnitude;
            emission.rateOverTimeMultiplier = m_FloorParticlesBaseEmission * multiplier;
        }
    }
}
}