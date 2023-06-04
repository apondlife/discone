using UnityEngine;

namespace ThirdPerson {

/// the character's grounded movement smoke effects
public sealed class CharacterGroundDust: MonoBehaviour {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("a curve of acceleration dir to dust amount")]
    [SerializeField] AnimationCurve m_AcclerationDirRate;

    [Tooltip("a curve of acceleration mag to dust amount")]
    [SerializeField] MapCurve m_AccelerationMagRate;

    [Tooltip("a curve of acceleration mag to dust size")]
    [SerializeField] MapCurve m_AccelerationMagSize;

    [Tooltip("a curve of acceleration mag to dust lifetime")]
    [SerializeField] MapCurve m_AccelerationMagLifetime;

    // -- refs --
    [Header("refs")]
    [Tooltip("the floor dust particle emitter (high speed)")]
    [SerializeField] ParticleSystem m_Particles;

    // -- props --
    /// the character's state
    CharacterState m_State;


    // -- lifecycle --
    void Start() {
        var character = GetComponentInParent<Character>();

        // set deps
        m_State = character.State;

        // play the particles foreer
        m_Particles.Play();
    }

    void FixedUpdate() {
        // start particles facing forward
        m_Particles.transform.forward = m_State.Forward;

        // get ground acceleration state
        var a = Vector3.ProjectOnPlane(
            m_State.Curr.Acceleration,
            m_State.Ground.Normal
        );

        var aDotV = Vector3.Dot(
            a,
            m_State.Velocity.normalized
        );

        var aMag = a.magnitude;

        // normalize a • v
        aDotV /= aMag;

        // set emission
        var emission = m_Particles.emission;

        // ...rate (strong when changing direction quickly)
        var rate = 0f;
        rate += m_AcclerationDirRate.Evaluate(aDotV);
        rate *= m_AccelerationMagRate.Evaluate(aMag);
        emission.rateOverTime = rate;

        // set main
        var main = m_Particles.main;

        // ...lifetime (long when accelerating quickly)
        main.startLifetime = m_AccelerationMagLifetime.Evaluate(aMag);

        // ...size (large when accelerating quickly)
        main.startSize = m_AccelerationMagSize.Evaluate(aMag);
    }
}

}