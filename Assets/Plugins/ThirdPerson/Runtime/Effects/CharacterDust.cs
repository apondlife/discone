using Soil;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {

// TODO: rename me to CharacterEffects & break me up into multiple prefabs & scripts
/// the character's dust effect
public class CharacterDust: MonoBehaviour {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the minimum negative acceleration to start skidding")]
    [SerializeField] float m_SkidDeceleration;

    [Tooltip("how many particles created from each unit of deceleration per frame")]
    [SerializeField] float m_AccelerationToDust = 0.01f;

    [Tooltip("how many particles on ground hit per unit of deceleration")]
    [FormerlySerializedAs("m_GroundAccelerationToDust")]
    [SerializeField] float m_LandingAccelerationToDust = 0.01f;

    // -- refs --
    [Header("refs")]
    [Tooltip("the floor skid lines particle (negative acceleration)")]
    [SerializeField] ParticleSystem m_FloorSkid;

    [Tooltip("the pivot particle emitter")]
    [SerializeField] ParticleSystem m_PivotParticles;

    // -- props --
    /// the containing character
    CharacterContainer c;

    // -- lifecycle --
    void Start() {
        c = GetComponentInParent<CharacterContainer>();
    }

    void FixedUpdate() {
        var next = c.State.Next;
        if (next.IsOnGround) {
            // check for deceleration, used for both skid and pivot dust
            var groundAcceleration = Vector3.ProjectOnPlane(next.Acceleration, next.MainSurface.Normal);
            var isDecelerating = Vector3.Dot(next.Velocity.normalized, groundAcceleration) < m_SkidDeceleration;

            // get the current surface
            var nextSurface = next.MainSurface;
            var surfaceScale = Mathx.Evaluate(c.Tuning.Surface_AngleScale.Curve, nextSurface.Angle);

            // check for character deceleration
            if (nextSurface.IsSome && (next.IsCrouching || isDecelerating)) {
                m_FloorSkid.Play();
                var t = m_FloorSkid.transform;
                t.position = nextSurface.Point;
                t.forward = -nextSurface.Normal;
            } else {
                m_FloorSkid.Stop();
            }

            // pivot effects
            if (isDecelerating) {
                m_PivotParticles.transform.forward = -next.Acceleration.normalized;
                var dustCount = Mathf.FloorToInt(1f - surfaceScale * m_AccelerationToDust * next.Acceleration.magnitude);
                m_PivotParticles.Emit(dustCount);
            }
        } else {
            if (m_FloorSkid.isPlaying) {
                m_FloorSkid.Stop();
            }
        }
    }
}

}