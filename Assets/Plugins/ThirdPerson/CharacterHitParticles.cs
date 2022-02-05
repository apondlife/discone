using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThirdPerson {
public class CharacterHitParticles : MonoBehaviour
{
    [SerializeField] private ParticleSystem m_WallParticles;
    [SerializeField] private CharacterState m_State;

    // Update is called once per frame
    void Update()
    {
        if(m_WallParticles.isPlaying && !m_State.IsOnWall) {
            m_WallParticles.Stop();
        }

        if(m_State.IsOnWall) {
            if(!m_WallParticles.isPlaying) {
                m_WallParticles.Play();
            }
            if(m_State.Collision.HasValue) {
                m_WallParticles.transform.position = m_State.Collision.Value.Point;
                m_WallParticles.transform.forward = -m_State.Collision.Value.Normal;
            }
        }
    }
}
}