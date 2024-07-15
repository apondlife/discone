using System;
using Soil;
using UnityEngine;

namespace ThirdPerson {

/// the character landing plume effect
/// -> character, world, once, change, ephemeral
[Serializable]
public class LandingPlume: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the particle system")]
    [SerializeField] ParticleSystem m_Particles;

    // -- props --
    /// the character container
    CharacterContainer c;

    /// the world's accumulated inertia
    float m_Inertia;

    /// the particle system start speed range
    ParticleSystem.MinMaxCurve m_StartSpeed;

    // -- lifecycle --
    public void Start() {
        // set deps
        c = GetComponentInParent<CharacterContainer>();

        // set props
        m_StartSpeed = m_Particles.main.startSpeed;
    }

    public void FixedUpdate() {
        var delta = Time.deltaTime;

        var next = c.State.Next;
        var surface = next.MainSurface;

        var tuning = c.Tuning.Model.LandingPlume;
        var curr = c.State.Curr;

        // only add inertia from first collision
        var inertia = m_Inertia;
        if (surface.IsSome && c.State.Prev.MainSurface.IsNone) {
            inertia += curr.Inertia;
        }

        // clamp decay so it doesn't bounce
        var inertiaDecay = inertia * Math.Min(tuning.InertiaDecay * delta, 1f);
        inertia -= inertiaDecay;

        // emit particles
        if (surface.IsSome) {
            var trs = m_Particles.transform;
            trs.position = surface.Point;
            trs.rotation = Quaternion.LookRotation(next.SurfaceDirection, surface.Normal);

            if (inertiaDecay > tuning.MinInertia) {
                var count = (int)tuning.InertiaToEmission.Evaluate(inertiaDecay);

                var main = m_Particles.main;
                main.startLifetime = tuning.InertiaToLifetime.Evaluate(inertiaDecay);
                main.startSize = tuning.InertiaToSize.Evaluate(inertiaDecay);

                var startSpeedScale = tuning.InertiaToStartSpeedScale.Evaluate(inertiaDecay);
                var startSpeed = m_StartSpeed;
                startSpeed.constantMin *= startSpeedScale;
                startSpeed.constantMax *= startSpeedScale;
                main.startSpeed = startSpeed;

                m_Particles.Emit(count);
                DebugDraw.Push("inertia_plume", c.State.Curr.Position, Vector3.up * m_Inertia, new DebugDraw.Config(Soil.Color.OrangeRed));

                Log.Model.I($"inertia: {inertia} -> {inertiaDecay}");
            }
        }

        // update inertia
        m_Inertia = inertia;
    }
}

}