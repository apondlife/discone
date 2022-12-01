using UnityEngine;

namespace ThirdPerson {

/// the character's speed lines effect
public class CharacterSpeedLines: MonoBehaviour {
    // -- tunables --
    [Header("tunables")]
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
    /// the character's state
    CharacterState m_State;

    // -- lifecycle --
    void Start() {
        // set deps
        var character = GetComponentInParent<Character>();
        m_State = character.State;

        // set initial state
        var emission = m_Particles.emission;
        emission.rateOverTime = 0.0f;
    }

    void FixedUpdate() {
        // attach to the anchor
        if (m_Anchor != null) {
            transform.position = m_Anchor.position;
        }

        var state = m_State.Curr;
        var v = state.GroundVelocity;
        var dir = v.normalized;

        // scale speed line based on ground speed
        var main = m_Particles.main;

        var speedTarget = v.sqrMagnitude * m_SpeedScale;
        var speed = Mathf.MoveTowards(
            main.startSpeed.constant,
            speedTarget,
            m_MaxSpeedDelta * Time.deltaTime
        );

        main.startSpeed = speed;

        // rotate speed line emitter opposite planar movement
        if (v != Vector3.zero) {
            transform.forward = Vector3.RotateTowards(transform.forward, dir, m_MaxRotationDelta * Time.deltaTime, 0.0f);
        }

        // turn lines as character accelerates
        var a = state.Acceleration;
        var la = transform.InverseTransformVector(a);
        var vol = m_Particles.velocityOverLifetime;
        var orbitalTarget = new Vector2(
            la.y,
            -la.x
        ) * m_RotationScale * m_SpeedScale;

        var orbital = Vector2.MoveTowards(
            new Vector2(vol.orbitalX.constant, vol.orbitalY.constant),
            orbitalTarget,
            m_MaxOrbitDelta * Time.deltaTime
        );

        vol.orbitalX = orbital.x;
        vol.orbitalY = orbital.y;
    }
}

}