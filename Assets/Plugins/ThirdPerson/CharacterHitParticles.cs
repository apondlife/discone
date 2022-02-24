using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThirdPerson {
public class CharacterHitParticles : MonoBehaviour
{
    [Header("tunables")]
    [Tooltip("the floor particle emission per unit of speed")]
    [SerializeField] private float m_FloorParticlesBaseEmission;

    [Header("references")]
    [SerializeField] private ParticleSystem m_WallParticles;
    [SerializeField] private ParticleSystem m_FloorParticles;
    [SerializeField] private ThirdPerson m_Character;

    // Update is called once per frame
    void Update()
    {
        if(m_WallParticles.isPlaying && !m_Character.IsOnWall) {
            m_WallParticles.Stop();
        }

        if(m_Character.IsOnWall) {
            if(!m_WallParticles.isPlaying) {
                m_WallParticles.Play();
            }
            if(m_Character.Collision.HasValue) {
                m_WallParticles.transform.position = m_Character.Collision.Value.Point;
                m_WallParticles.transform.forward = -m_Character.Collision.Value.Normal;
            }
        }

        if(m_FloorParticles.isPlaying && !m_Character.IsGrounded) {
            m_FloorParticles.Stop();
        }

        if(m_Character.IsGrounded) {
            if(!m_FloorParticles.isPlaying) {

                m_FloorParticles.Play();
            }
            if(m_Character.Collision.HasValue) {
                    m_FloorParticles.transform.position = m_Character.Collision.Value.Point;
                }

            var emission = m_FloorParticles.emission;
            var multiplier = m_Character.PlanarVelocity.sqrMagnitude;
            emission.rateOverTimeMultiplier = m_FloorParticlesBaseEmission * multiplier;
        }
    }
}
}