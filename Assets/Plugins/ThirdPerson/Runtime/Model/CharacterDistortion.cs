using System.Collections.Generic;
using System.Linq;
using Soil;
using UnityEngine;

namespace ThirdPerson {

// [ExecuteAlways]
sealed class CharacterDistortion: MonoBehaviour {
    // -- config --
    [Tooltip("the position for distorting from above the character")]
    [SerializeField] Vector3 m_Top;

    // -- fields --
    [Header("fields")]
    [Tooltip("the distortion scale; 0 is fully squashed, 1 is no distortion, infinity is infinitely stretched.")]
    [SerializeField] float m_Intensity;

    [Tooltip("the distortion scale on the negative edge of the plane")]
    [SerializeField] float m_NegativeScale;

    [Tooltip("the distortion scale on the positive edge of the plane")]
    [SerializeField] float m_PositiveScale;

    // -- stretch & squash --
    [Header("tuning")]
    [Tooltip("TODO: leave me a comment")]
    [SerializeField] MapOutCurve m_JumpSquat_Intensity;

    [Tooltip("TODO: leave me a comment")]
    [SerializeField] float m_VerticalAcceleration_StretchIntensity;

    [Tooltip("TODO: leave me a comment")]
    [SerializeField] float m_VerticalAcceleration_SquashIntensity;

    [Tooltip("TODO: leave me a comment")]
    [SerializeField] float m_VerticalAcceleration_IntensitySpeed;

    [Tooltip("TODO: leave me a comment")]
    [SerializeField] float m_VerticalAcceleration_IntensityDamp;

    // -- props --
    /// .
    CharacterState m_State;

    /// .
    CharacterTuning m_Tuning;

    // the list of distorted materials
    Material[] m_Materials;

    /// the current distortion intensity speed
    public float m_IntensitySpeed = 0f;

    // -- lifetime --
    void Awake() {
        // set initial values
        m_Intensity = 1f;
    }

    void Start() {
        // set deps
        var c = GetComponentInParent<CharacterContainer>();
        m_State = c.State;
        m_Tuning = c.Tuning;

        // aggregate a list of materials
        var materials = new HashSet<Material>();
        var renderers = c.Model
            .GetComponentsInChildren<Renderer>(true);

        foreach (var renderer in renderers) {
            materials.UnionWith(renderer.materials);
        }

        m_Materials = materials.ToArray();
    }

    void FixedUpdate() {
        StretchAndSquash();
        Distort();
    }

    /// change character scale according to acceleration
    public float destIntensity = 1f;
    void StretchAndSquash() {
        var v = m_State.Prev.Velocity.y;
        var a = m_State.Curr.Acceleration.y;
        var aMag = Mathf.Abs(a);

        // when jumping, move a distortion plane to our feet
        // if (v >= 0 || m_State.Next.IsOnGround) {
            transform.localPosition = Vector3.zero;
            transform.up = Vector3.up;
        // }
        // when falling, move a distortion plane above our head
        // else {
        //     transform.localPosition = m_Top;
        //     transform.up = Vector3.down;
        // }

        // determine dest intensity
        destIntensity = 1f;

        // if in jump squat, add jump squash
        if (m_State.Next.IsInJumpSquat) {
            var jumpTuning = m_Tuning.NextJump(m_State);
            var jumpPower = jumpTuning.Power(m_State.Next.JumpState.PhaseElapsed);

            destIntensity = m_JumpSquat_Intensity.Evaluate(jumpPower);
        }
        // otherwise, stretch/squash based on vertical the acceleration/velocity relationship
        else {
            // if accelerating against velocity, sigh should squash (negative sign), otherwise, stretch
            var aDotV = a * Mathf.Sign(v);
            var scaledADotB = m_VerticalAcceleration_StretchIntensity * aDotV;

            // sigmoid (thanks paradise)
            // https://www.desmos.com/calculator/stfjbdj5lh
            destIntensity = 1 + (2 / Mathf.PI) * Mathf.Atan(
                scaledADotB * Mathf.Pow(
                    Mathf.Abs(scaledADotB),
                    m_VerticalAcceleration_SquashIntensity
                )
            );

            // stretch: accelerating the same as velocity
            // squash: accelerating opposite to velocity
            // else {
                // destIntensity = 1 - m_VerticalAcceleration_SquashIntensity / aMag;
            // }
        }

        var intensityAcceleration = m_VerticalAcceleration_IntensitySpeed * (destIntensity - m_Intensity);
        m_IntensitySpeed += intensityAcceleration * Time.deltaTime;
        m_Intensity += m_IntensitySpeed * Time.deltaTime;
        m_IntensitySpeed *= m_VerticalAcceleration_IntensityDamp;
    }

    // -- commands --
    void Distort() {
        var plane = new Plane(transform.up, transform.position).AsVector4();

        foreach (var material in m_Materials) {
            material.SetVector(
                ShaderProps.Distortion_Plane,
                plane
            );

            material.SetFloat(
                ShaderProps.Distortion_Intensity,
                m_Intensity
            );

            material.SetFloat(
                ShaderProps.Distortion_PositiveScale,
                m_NegativeScale
            );

            material.SetFloat(
                ShaderProps.Distortion_NegativeScale,
                m_NegativeScale
            );
        }
    }
}

}