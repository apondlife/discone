using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {

/// the character's dust effect
/// TODO: rename me to CharacterEffects
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

    [Tooltip("the plume when jump starts")]
    [SerializeField] ParticleSystem m_JumpPlume;

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
        // TODO: move into own script
        if (m_State.Next.Events.Contains(CharacterEvent.Jump)) {
            m_JumpPlume.Play();
        }

        if (m_State.Next.IsOnWall) {
            if (!m_WallParticles.isPlaying) {
                m_WallParticles.Play();
            }

            var c = m_State.Wall.IsSome ? m_State.Wall : m_State.Ground;
            var t = m_WallParticles.transform;
            t.position = c.Point;

            // AAA: help here
            var main = m_WallParticles.main;

            var n = c.Normal;
            n.z = -n.z;
            var r = Quaternion.LookRotation(n).eulerAngles * Mathf.Deg2Rad;

            main.startRotationX = r.x;
            main.startRotationY = r.y;
            main.startRotationZ = r.z;
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