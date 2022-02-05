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

        if(!m_WallParticles.isPlaying && m_State.IsOnWall) {
            m_WallParticles.Play();
        }

    }
}
}