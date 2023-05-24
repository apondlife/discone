using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {

/// the character's dust effect
public class CharacterDust: MonoBehaviour {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the floor particle emission per unit of speed")]
    [SerializeField] float m_FloorParticlesBaseEmission;

    [Tooltip("the minimum negative acceleration to start skidding")]
    [SerializeField] float m_SkidDeceleration;

    [Tooltip("how many particles created from each unit of deceleration per frame")]
    [SerializeField] float m_AccelerationToDust = 0.01f;

    [Tooltip("how many particles on ground hit per unit of deceleration")]
    [FormerlySerializedAs("m_GroundAccelerationToDust")]
    [SerializeField] float m_LandingAccelerationToDust = 0.01f;

    // -- refs --
    [Header("refs")]
    [Tooltip("the wall particle emitter")]
    [SerializeField] ParticleSystem m_WallParticles;

    [Tooltip("the floor skid lines particle (negative acceleration)")]
    [SerializeField] ParticleSystem m_FloorSkid;

    [Tooltip("the particle puff when landing")]
    [SerializeField] ParticleSystem m_LandingPuff;

    [Tooltip("the particle stamp (on the ground) when landing")]
    [SerializeField] ParticleSystem m_LandingStamp;

    [Tooltip("the pivot particle emitter")]
    [SerializeField] ParticleSystem m_PivotParticles;

    // -- props --
    /// the character's state
    CharacterState m_State;

    // -- lifecycle --
    void Start() {
        var character = GetComponentInParent<Character>();
        m_State = character.State;
    }

    void FixedUpdate() {
        if (m_State.IsOnWall) {
            if (!m_WallParticles.isPlaying) {
                m_WallParticles.Play();
            }

            if (!m_State.Wall.IsNone) {
                var c = m_State.Wall;
                var t = m_WallParticles.transform;
                t.position = c.Point;
                t.forward = -c.Normal;
            }
        } else {
            if (m_WallParticles.isPlaying) {
                m_WallParticles.Stop();
            }
        }

        if (m_State.Next.IsOnGround) {
            // check for deceleration, used for both skid and pivot dust
            var groundAcceleration = Vector3.ProjectOnPlane(m_State.Acceleration, m_State.Ground.Normal);
            var isDecelerating = Vector3.Dot(m_State.Velocity.normalized, groundAcceleration) < m_SkidDeceleration;

            // check for character deceleration
            if (m_State.IsCrouching || isDecelerating) {
                m_FloorSkid.Play();
                var c = m_State.Ground;
                var t = m_FloorSkid.transform;
                t.position = c.Point;
                t.forward = -c.Normal;
            } else {
                m_FloorSkid.Stop();
            }

            // pivot effects
            if (isDecelerating) {
                m_PivotParticles.transform.forward = -m_State.Acceleration.normalized;
                var dustCount = Mathf.FloorToInt(m_AccelerationToDust * m_State.Acceleration.magnitude);
                m_PivotParticles.Emit(dustCount);
            }
        } else {
            if (m_FloorSkid.isPlaying) {
                m_FloorSkid.Stop();
            }
        }

        // if just landed
        if (m_State.Next.IsOnGround) {
            m_LandingPuff.transform.up = m_State.Ground.Normal;
            m_LandingPuff.transform.position = m_State.Ground.Point;
            var groundForce = Vector3.Project(m_State.Acceleration, m_State.Ground.Normal).magnitude;
            var particles = Mathf.FloorToInt(m_LandingAccelerationToDust * groundForce);
            m_LandingPuff.Emit(particles);
            m_LandingStamp.Emit(particles);
        }
    }
}

}