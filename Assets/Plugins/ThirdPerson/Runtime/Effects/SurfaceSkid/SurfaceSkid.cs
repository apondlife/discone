using System;
using Soil;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ThirdPerson {

/// the skid effect when sliding along wall-like surfaces
sealed class SurfaceSkid: CharacterEffect {
    // -- constants --
    /// the bits sub-emitter index
    const int k_Bits = 0;

    /// the burn sub-emitter index
    const int k_Burn = 1;

    // -- refs --
    [Header("refs")]
    [Tooltip("the particle emitter")]
    [SerializeField] ParticleSystem m_SkidParticles;

    // -- lifecycle --
    protected override void Awake() {
        base.Awake();

        // setup color texture
        InitColorTexture(m_SkidParticles);
    }

    void FixedUpdate() {
        var next = c.State.Next;
        var curr = c.State.Curr;

        // only play particles when wall & not idle
        if (!next.IsOnWall || next.IsIdle) {
            return;
        }

        // get tuning
        var tuning = c.Tuning.Model.SurfaceSkid;

        // get surface
        var surface = next.MainSurface;

        // get emitters
        var skid = m_SkidParticles;
        var bits = skid.subEmitters.GetSubEmitterSystem(k_Bits);
        var burn = skid.subEmitters.GetSubEmitterSystem(k_Burn);

        // spawn at collision
        var trs = skid.transform;
        trs.position = surface.Point;

        // point away from the current surface and rotate a bit around its normal
        var rot = Quaternion.LookRotation(-surface.Normal, next.Forward);
        rot *= Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward);

        // shape needs euler in degrees
        var euler = rot.eulerAngles;
        var shapeFwd = rot * Vector3.forward;

        // rotate the emission shapes
        var shape = skid.shape;
        shape.position = -tuning.Offset * 1 * shapeFwd;
        shape.rotation = euler;

        shape = bits.shape;
        shape.position = shape.position;
        shape.rotation = euler;

        shape = burn.shape;
        shape.position = -tuning.Offset * 2 * shapeFwd;
        shape.rotation = euler;

        // particles need euler in radians
        var minEuler = euler * Mathf.Deg2Rad;
        var maxEuler = (rot * Quaternion.AngleAxis(180f, Vector3.forward)).eulerAngles * Mathf.Deg2Rad;

        var rotX = new ParticleSystem.MinMaxCurve(minEuler.x, maxEuler.x);
        var rotY = new ParticleSystem.MinMaxCurve(minEuler.y, maxEuler.y);
        var rotZ = new ParticleSystem.MinMaxCurve(minEuler.z, maxEuler.z);

        // rotate the skid particle
        var main = skid.main;

        // face +z, instead of the -z (the particle system default)
        main.flipRotation = 1f;
        main.startRotationX = rotX;
        main.startRotationY = rotY;
        main.startRotationZ = rotZ;

        // sync color texture
        SyncColorTexture(skid);

        // emit
        var count = tuning.SqrSpeedToCount.Evaluate(next.SurfaceVelocity.sqrMagnitude);
        skid.Emit((int)count);

        // configure burn
        var inertiaDecay = Mathf.Max(curr.Inertia - next.Inertia, 0f);

        main = burn.main;
        main.startSizeMultiplier = tuning.Burn_InertiaDecayToSize.Evaluate(inertiaDecay);
        main.startLifetimeMultiplier = tuning.Burn_InertiaDecayToLifetime.Evaluate(inertiaDecay);

        // face +z, instead of the -z (the particle system default)
        main.flipRotation = 1f;
        main.startRotationX = rotX;
        main.startRotationY = rotY;
        main.startRotationZ = rotZ;

        // emit burn
        count = tuning.Burn_InertiaDecayToCount.Evaluate(inertiaDecay);
        burn.Emit((int)count);
    }
}

}