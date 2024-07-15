using Soil;
using UnityEngine;

namespace ThirdPerson {

/// the cloud of dust when the character jumps
public class JumpPlume: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("emission count as a fn of sqr speed delta")]
    [SerializeField] MapCurve m_SqrSpeedToEmission;

    [Tooltip("particle size as a fn of sqr speed delta")]
    [SerializeField] MapCurve m_SqrSpeedToSize;

    [Tooltip("lifetime as a fn of sqr speed delta")]
    [SerializeField] MapCurve m_SqrSpeedToLifetime;

    [Tooltip("start speed as a fn of sqr speed delta")]
    [SerializeField] MapCurve m_SqrSpeedToStartSpeedScale;

    // -- refs --
    [Header("refs")]
    [Tooltip("the particle system")]
    [SerializeField] ParticleSystem m_Particles;

    // -- props --
    /// the character container
    CharacterContainer c;

    /// the particle system start speed range
    ParticleSystem.MinMaxCurve m_StartSpeed;

    // -- lifecycle --
    void Start() {
        // set deps
        c = GetComponentInParent<CharacterContainer>();

        // set props
        m_StartSpeed = m_Particles.main.startSpeed;
    }

    void FixedUpdate() {
        var next = c.State.Next;
        if (!next.Events.Contains(CharacterEvent.Jump)) {
            return;
        }

        var dv = next.Velocity - c.State.Curr.Velocity;
        m_Particles.transform.up = c.State.Next.PerceivedSurface.Normal;

        // TODO: get actual jump speed
        var sqrSpeed = Vector3.SqrMagnitude(dv);
        var count = (int)m_SqrSpeedToEmission.Evaluate(sqrSpeed);

        var main = m_Particles.main;
        main.startLifetime = m_SqrSpeedToLifetime.Evaluate(sqrSpeed);
        main.startSize = m_SqrSpeedToSize.Evaluate(sqrSpeed);

        var startSpeedScale = m_SqrSpeedToStartSpeedScale.Evaluate(sqrSpeed);
        var startSpeed = m_StartSpeed;
        startSpeed.constantMin *= startSpeedScale;
        startSpeed.constantMax *= startSpeedScale;
        main.startSpeed = startSpeed;

        m_Particles.Emit(count);
    }
}
}