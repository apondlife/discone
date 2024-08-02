using Soil;
using UnityEngine;

namespace ThirdPerson {

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

        // only play particles when wall & not idle
        if (!next.IsOnWall || next.IsIdle) {
            return;
        }

        // get surface
        var surface = next.MainSurface;

        // spawn at collision
        var trs = m_Particles.transform;
        trs.position = surface.Point;

        // point away from the current surface and rotate a bit around its normal
        var rot = Quaternion.LookRotation(-surface.Normal, next.Forward);
        rot *= Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward);

        // shape/particle needs euler in degrees/radians respectively
        var euler = rot.eulerAngles;

        // rotate the emission shapes
        var shape = m_Particles.shape;
        shape.rotation = euler;

        var subShape = m_Particles.subEmitters.GetSubEmitterSystem(0).shape;
        subShape.rotation = euler;

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

        // emit particles
        var count = (int)m_Count.Evaluate(next.SurfaceVelocity.sqrMagnitude);
        m_Particles.Emit(count);
    }
}

}