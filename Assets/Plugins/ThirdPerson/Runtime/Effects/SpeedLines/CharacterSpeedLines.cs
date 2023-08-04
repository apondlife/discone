using UnityEngine;

namespace ThirdPerson {

/// the character's speed lines effect
sealed class CharacterSpeedLines: MonoBehaviour {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the scale for start speed as a fn of sqr velocity")]
    [SerializeField] float m_SpeedScale;

    [Tooltip("the scale for rotation over time as a fn acceleration")]
    [SerializeField] Vector2 m_RotationScale;

    [Tooltip("the max speed change per second")]
    [SerializeField] float m_MaxSpeedDelta;

    [Tooltip("the max orbit speed change per second")]
    [SerializeField] float m_MaxOrbitDelta;

    [Tooltip("the max rotation change per second")]
    [SerializeField] float m_MaxRotationDelta;

    // -- refs --
    [Header("refs")]
    [Tooltip("the anchor transform")]
    [SerializeField] Transform m_Anchor;

    [Tooltip("the particle that shows horizontal speed")]
    [SerializeField] ParticleSystem m_Particles;

    // -- props --
    /// the character container
    CharacterContainer c;

    // -- lifecycle --
    void Start() {
        // set deps
        this.c = GetComponentInParent<CharacterContainer>();

        // set initial state
        var emission = m_Particles.emission;
        emission.rateOverTime = 0.0f;
    }

    void FixedUpdate() {
        // attach to the anchor
        if (m_Anchor != null) {
            transform.position = m_Anchor.position;
        }

        var v = c.State.Next.GroundVelocity;
        var dir = v.normalized;

        // scale speed line based on ground speed
        var main = m_Particles.main;

        var destSpeed = v.sqrMagnitude * m_SpeedScale;
        var nextSpeed = Mathf.MoveTowards(
            main.startSpeed.constant,
            destSpeed,
            m_MaxSpeedDelta * Time.deltaTime
        );

        main.startSpeed = nextSpeed;

        // rotate speed line emitter opposite planar movement
        if (v != Vector3.zero) {
            transform.forward = Vector3.RotateTowards(
                transform.forward,
                dir,
                m_MaxRotationDelta * Time.deltaTime,
                0.0f
            );
        }

        // rotate lines as character accelerates
        var vol = m_Particles.velocityOverLifetime;

        var a = transform.InverseTransformVector(c.State.Next.Acceleration);
        var destOrbital = new Vector2(a.y, -a.x) * m_RotationScale * m_SpeedScale;
        var nextOrbital = Vector2.MoveTowards(
            new Vector2(
                vol.orbitalX.constant,
                vol.orbitalY.constant
            ),
            destOrbital,
            m_MaxOrbitDelta * Time.deltaTime
        );

        vol.orbitalX = nextOrbital.x;
        vol.orbitalY = nextOrbital.y;
        vol.orbitalZ = 1f;
    }
}

}