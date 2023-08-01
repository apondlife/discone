using UnityEngine;

namespace ThirdPerson {

/// the character's grounded movement smoke effects
sealed class CharacterGroundDust: MonoBehaviour {
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
    /// the character container
    CharacterContainer c;

    // -- lifecycle --
    void Start() {
        // set deps
        this.c = GetComponentInParent<Character>();

        // play the particles foreer
        m_Particles.Play();
    }

    void FixedUpdate() {
        // start particles facing forward
        m_Particles.transform.forward = c.State.Next.Forward;

        // get ground acceleration state
        var a = Vector3.ProjectOnPlane(
            c.State.Curr.Acceleration,
            c.State.Next.Ground.Normal
        );

        var aDotV = Vector3.Dot(
            a,
            c.State.Next.Velocity.normalized
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