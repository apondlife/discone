using Soil;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {

sealed class CharacterDistortion: MonoBehaviour {
    // -- config --
    [Tooltip("the position for distorting from above the character")]
    [SerializeField] Vector3 m_Top;

    [Header("tuning")]
    [FormerlySerializedAs("m_PositiveScale")]
    [Tooltip("a scale on intensity along the plane's axis")]
    [SerializeField] float m_AxialScale;

    [FormerlySerializedAs("m_NegativeScale")]
    [Tooltip("a scale on intensity around the plane's axis (inversely proportional to axial)")]
    [SerializeField] float m_RadialScale;

    [Tooltip("the stretch and squash intensity acceleration scale, 0 full squash, 1 no distortion, infinity infinitely stretched")]
    [SerializeField] FloatRange m_Intensity_Acceleration;

    [Tooltip("the stretch and squash intensity velocity scale, 0 full squash, 1 no distortion, infinity infinitely stretched")]
    [SerializeField] FloatRange m_Intensity_Velocity;

    [FormerlySerializedAs("m_Steepness")]
    [Tooltip("the responsiveness of the movement based intensity")]
    [SerializeField] float m_Responsiveness;

    [Tooltip("the intensity ease on acceleration based stretch & squash")]
    [SerializeField] DynamicEase<float> m_Ease;

    // -- props --
    /// .
    CharacterContainer c;

    // -- lifecycle --
    void Start() {
        c = GetComponentInParent<CharacterContainer>();

        // initialize ease
        m_Ease.Init(1f);
    }

    void FixedUpdate() {
        var delta = Time.deltaTime;
        StretchAndSquash(delta);
        Distort();
    }

    /// change character scale according to acceleration
    void StretchAndSquash(float delta) {
        var v = c.State.Prev.Velocity.y;
        var a = c.State.Curr.Acceleration.y * delta;

        // determine dest intensity
        var destIntensity = 1f;

        // if in jump squat, add jump squash
        if (c.State.Next.IsInJumpSquat) {
            var jumpId = c.State.Next.NextJump;
            var jumpSquatElapsed = c.State.Next.JumpState.PhaseElapsed;

            var jumpSquashIntensity = 0f;

            // use the squash curve if available
            var jumpTuning = c.Tuning.Model.JumpById(jumpId);
            if (jumpTuning != null) {
                jumpSquashIntensity = jumpTuning.Squash.Evaluate(jumpSquatElapsed);
            }
            // otherwise, just use raw power
            else {
                jumpSquashIntensity = c.Tuning.JumpById(jumpId).Power(jumpSquatElapsed);
            }

            destIntensity = jumpSquashIntensity;
        }
        // otherwise, stretch/squash based on vertical the acceleration/velocity relationship
        else {
            // if accelerating against velocity, sign should squash (negative sign), otherwise, stretch
            var ia = m_Intensity_Acceleration.Evaluate(Mathf.Sign(v * a) * 0.5f + 0.5f) * Mathf.Abs(a);
            var iv = m_Intensity_Velocity.Evaluate(Mathf.Sign(v) * 0.5f + 0.5f) * Mathf.Abs(v);

            // TODO: maybe spring this instead of the sigmoid?
            var intensityRaw = ia + iv;

            // sigmoid (thanks paradise)
            // https://www.desmos.com/calculator/stfjbdj5lh
            destIntensity = 1f + (2f / Mathf.PI) * Mathf.Atan(
                intensityRaw * Mathf.Pow(
                    Mathf.Abs(intensityRaw),
                    m_Responsiveness
                )
            );
        }

        m_Ease.Update(delta, destIntensity);
    }

    // -- commands --
    void Distort() {
        var plane = new Plane(transform.up, transform.position).AsVector4();

        foreach (var material in c.Model.Materials.All) {
            material.SetVector(
                ShaderProps.Character_Pos,
                c.State.Next.Position
            );

            material.SetVector(
                ShaderProps.Distortion_Plane,
                plane
            );

            material.SetFloat(
                ShaderProps.Distortion_Intensity,
                Mathf.Max(m_Ease.Value, 0f)
            );

            material.SetFloat(
                ShaderProps.Distortion_AxialScale,
                m_AxialScale
            );

            material.SetFloat(
                ShaderProps.Distortion_RadialScale,
                m_RadialScale
            );
        }
    }
}

}