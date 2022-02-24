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
    /// the character's tate
    CharacterState m_State;

    // -- lifecycle --
    void Awake() {
        var container = GetComponentInParent<ThirdPerson>();
        m_State = container.State;
    }

    void Update() {
        if(m_WallParticles.isPlaying && !m_State.IsOnWall) {
            m_WallParticles.Stop();
        }

        if(m_State.IsOnWall) {
            if(!m_WallParticles.isPlaying) {
                m_WallParticles.Play();
            }

            if(m_State.Collision.HasValue) {
                var c = m_State.Collision;
                var t = m_WallParticles.transform;
                t.position = c.Value.Point;
                t.forward = -c.Value.Normal;
            }
        }

        if(m_FloorParticles.isPlaying && !m_State.IsGrounded) {
            m_FloorParticles.Stop();
        }

        if(m_State.IsGrounded) {
            if(!m_FloorParticles.isPlaying) {
                m_FloorParticles.Play();
            }

            if(m_State.Collision.HasValue) {
                var t = m_FloorParticles.transform;
                var c = m_State.Collision.Value;
                t.position = c.Point;
            }

            var emission = m_FloorParticles.emission;
            var multiplier = m_State.PlanarVelocity.sqrMagnitude;
            emission.rateOverTimeMultiplier = m_FloorParticlesBaseEmission * multiplier;
        }
    }
}
}