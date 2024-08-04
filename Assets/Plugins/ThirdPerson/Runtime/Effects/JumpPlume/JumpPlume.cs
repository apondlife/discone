using Soil;
using UnityEngine;

namespace ThirdPerson {

/// the cloud of dust when the character jumps
public class JumpPlume: CharacterEffect {
    // -- refs --
    [Header("refs")]
    [Tooltip("the particle system")]
    [SerializeField] ParticleSystem m_Particles;

    // -- props --
    /// the particle system start speed range
    ParticleSystem.MinMaxCurve m_StartSpeed;

    // -- lifecycle --
    protected override void Awake() {
        base.Awake();

        // set props
        m_StartSpeed = m_Particles.main.startSpeed;
    }

    void FixedUpdate() {
        var next = c.State.Next;
        if (!next.Events.Contains(CharacterEvent.Jump)) {
            return;
        }

        var tuning = c.Tuning.Model.JumpPlume;

        var dv = next.Velocity - c.State.Curr.Velocity;
        m_Particles.transform.up = c.State.Next.PerceivedSurface.Normal;

        // TODO: get actual jump speed
        var sqrSpeed = Vector3.SqrMagnitude(dv);
        var count = (int)tuning.SqrSpeedToEmission.Evaluate(sqrSpeed);

        var main = m_Particles.main;
        main.startLifetime = tuning.SqrSpeedToLifetime.Evaluate(sqrSpeed);
        main.startSize = tuning.SqrSpeedToSize.Evaluate(sqrSpeed);

        var startSpeedScale = tuning.SqrSpeedToStartSpeedScale.Evaluate(sqrSpeed);
        var startSpeed = m_StartSpeed;
        startSpeed.constantMin *= startSpeedScale;
        startSpeed.constantMax *= startSpeedScale;
        main.startSpeed = startSpeed;

        // emit particles
        m_Particles.Emit(count);
    }
}

}