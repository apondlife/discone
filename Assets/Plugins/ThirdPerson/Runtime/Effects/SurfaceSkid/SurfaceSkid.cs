using Soil;
using UnityEngine;

namespace ThirdPerson {

// AAA: fix this next

/// the skid effect when sliding along wall-like surfaces
sealed class SurfaceSkid: CharacterEffect {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the number of particles per frame")]
    [SerializeField] MapCurve m_Count;

    // -- refs --
    [Header("refs")]
    [Tooltip("the wall particle emitter")]
    [SerializeField] ParticleSystem m_Particles;

    // -- lifecycle --
    protected override void Awake() {
        base.Awake();

        // setup color texture
        InitColorTexture(m_Particles);
    }

    void FixedUpdate() {
        var next = c.State.Next;
        var curr = c.State.Curr;

        // only play particles when wall & not idle
        if (!next.IsOnWall || next.IsIdle) {
            return;
        }

        // get surface
        var surface = next.MainSurface;

        // get subemitters
        var bits = m_Particles.subEmitters.GetSubEmitterSystem(0);
        var burn = m_Particles.subEmitters.GetSubEmitterSystem(1);

        // spawn at collision
        var trs = m_Particles.transform;
        trs.position = surface.Point;

        // point away from the current surface and rotate a bit around its normal
        var rot = Quaternion.LookRotation(-surface.Normal, next.Forward);
        rot *= Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward);

        // shape/particle needs euler in degrees/radians respectively
        var euler = rot.eulerAngles;

        // rotate the emission shapes
        var offset = -0.01f;
        var shape = m_Particles.shape;
        shape.position = offset * (rot * Vector3.forward);
        shape.rotation = euler;

        var bitsShape = bits.shape;
        bitsShape.position = shape.position;
        bitsShape.rotation = euler;

        offset += -0.01f;
        var burnShape = burn.shape;
        burnShape.position = offset * (rot * Vector3.forward);
        burnShape.rotation = euler;

        // rotate the particle
        var main = m_Particles.main;

        // face +z, instead of the -z (the particle system default)
        main.flipRotation = 1f;

        euler *= Mathf.Deg2Rad;
        main.startRotationX = euler.x;
        main.startRotationY = euler.y;
        main.startRotationZ = euler.z;

        // sync color texture
        SyncColorTexture(m_Particles);

        // emit burn
        // TODO: not a subemitter?
        var inertiaDecay = Mathf.Max(curr.Inertia - next.Inertia, 0f);

        var inertiaDecayToSizeScale = new MapCurve();
        var minBurnDecay = 0.05f;
        inertiaDecayToSizeScale.Src = new(minBurnDecay, 2f);
        inertiaDecayToSizeScale.Dst = new(0.15f, 2.4f);

        var inertiaDecayToLifetimeScale = new MapCurve();
        inertiaDecayToLifetimeScale.Src = new(1f, 2f);
        inertiaDecayToLifetimeScale.Dst = inertiaDecayToLifetimeScale.Src;
        inertiaDecayToLifetimeScale.Dst *= 4f;

        var inertiaDecayToValue = new MapCurve();
        inertiaDecayToValue.Src = new(minBurnDecay, 2f);
        inertiaDecayToValue.Dst = new(0f, -1f);

        var burnMain = burn.main;
        burnMain.startSizeMultiplier = inertiaDecayToSizeScale.Evaluate(inertiaDecay);

        var m_StartLifetime = 1f;
        burnMain.startLifetimeMultiplier = m_StartLifetime * inertiaDecayToLifetimeScale.Evaluate(inertiaDecay);

        euler *= Mathf.Deg2Rad;
        burnMain.startRotationX = euler.x;
        burnMain.startRotationY = euler.y;
        burnMain.startRotationZ = euler.z;

        // var m_StartColor = new Color(1f, 1f, 1f, 0.13f);
        // burn.startColor = m_StartColor.AddValue(inertiaDecayToValue.Evaluate(inertiaDecay));
        var count = (int)m_Count.Evaluate(next.SurfaceVelocity.sqrMagnitude);
        burn.Emit(count * (inertiaDecay > minBurnDecay ? 1 : 0));
        m_Particles.Emit(count);
    }
}

}