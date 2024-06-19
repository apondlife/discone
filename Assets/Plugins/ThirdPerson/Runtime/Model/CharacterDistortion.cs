using System;
using System.Collections.Generic;
using System.Linq;
using Soil;
using UnityEngine;
using UnityEngine.Serialization;
using Color = Soil.Color;

namespace ThirdPerson {

// [ExecuteAlways]
sealed class CharacterDistortion: MonoBehaviour {
    // -- config --
    [Tooltip("the position for distorting from above the character")]
    [SerializeField] Vector3 m_Top;

    // -- fields --
    [Header("fields")]

    [Tooltip("the distortion scale on the negative edge of the plane")]
    [SerializeField] float m_NegativeScale;

    [Tooltip("the distortion scale on the positive edge of the plane")]
    [SerializeField] float m_PositiveScale;

    // -- stretch & squash --
    [Header("tuning")]
    [Tooltip("the intensity of the jumpsquat stretch&squash, 0 full squash, 1 no distortion, infinity infinitely stretched")]
    [SerializeField] MapOutCurve m_JumpSquat_Intensity;

    //"the distortion scale; 0 is fully squashed, 1 is no distortion, infinity is infinitely stretched.")]
    [Tooltip("TODO: leave me a comment")]
    [SerializeField] float m_VerticalAcceleration_StretchIntensity;

    [Tooltip("TODO: leave me a comment")]
    [SerializeField] FloatRange m_Intensity_Acceleration;

    [Tooltip("TODO: leave me a comment")]
    [SerializeField] FloatRange m_Intensity_Velocity;

    //"the distortion scale; 0 is fully squashed, 1 is no distortion, infinity is infinitely stretched.")]
    [FormerlySerializedAs("m_VerticalAcceleration_SquashIntensity")]
    [Tooltip("TODO: leave me a comment")]
    [SerializeField] float m_Steepness;

    //"the distortion scale; 0 is fully squashed, 1 is no distortion, infinity is infinitely stretched.")]
    [FormerlySerializedAs("m_VerticalSpeed_StretchIntesity")]
    [Tooltip("TODO: leave me a comment")]
    [SerializeField] float m_VerticalSpeed_StretchIntensity;

    //"the distortion scale; 0 is fully squashed, 1 is no distortion, infinity is infinitely stretched.")]
    [Tooltip("TODO: leave me a comment")]
    [SerializeField] float m_VerticalSpeed_SquashIntesity;

    [Tooltip("the intensity ease on acceleration based stretch & squash")]
    [SerializeField] DynamicEase m_Ease;

    // -- props --
    /// .
    CharacterContainer c;

    // the list of distorted materials
    Material[] m_Materials;

    // -- lifetime --
    void Start() {
        // set deps
        c = GetComponentInParent<CharacterContainer>();

        // aggregate a list of materials
        var materials = new HashSet<Material>();
        var renderers = c.Model
            .GetComponentsInChildren<Renderer>(true);

        foreach (var renderer in renderers) {
            materials.UnionWith(renderer.materials);
        }

        m_Materials = materials.ToArray();

        // initialize ease
        m_Ease.Init(Vector3.up);
    }

    void FixedUpdate() {
        var delta = Time.deltaTime;
        StretchAndSquash(delta);
        Distort();
    }

    /// change character scale according to acceleration
    Vector3 pos;
    void StretchAndSquash(float delta) {
        var v = c.State.Prev.Velocity.y;
        var a = c.State.Curr.Acceleration.y * delta;
        var aMag = Mathf.Abs(a);

        // when jumping, move a distortion plane to our feet
        // if (v >= 0 || c.State.Next.IsOnGround) {
            transform.localPosition = Vector3.zero;
            transform.up = Vector3.up;
        // }
        // when falling, move a distortion plane above our head
        // else {
        //     transform.localPosition = m_Top;
        //     transform.up = Vector3.down;
        // }

        // determine dest intensity
        var destIntensity = 1f;
        var p0 = transform.position + Vector3.up;
        var p = p0;

        // if in jump squat, add jump squash
        if (c.State.Next.IsInJumpSquat) {
            var jumpTuning = c.Tuning.NextJump(c.State);
            var jumpPower = jumpTuning.Power(c.State.Next.JumpState.PhaseElapsed);

            destIntensity = m_JumpSquat_Intensity.Evaluate(jumpPower);
            p += Vector3.right * 1.3f;
            p += Vector3.right * 1.3f;
        }
        // otherwise, stretch/squash based on vertical the acceleration/velocity relationship
        else {
            // if accelerating against velocity, sign should squash (negative sign), otherwise, stretch
            var ia = m_Intensity_Acceleration.Evaluate(Mathf.Sign(v * a) * 0.5f + 0.5f) * Mathf.Abs(a);
            var iv = m_Intensity_Velocity.Evaluate(Mathf.Sign(v) * 0.5f + 0.5f) * Mathf.Abs(v);

            // TODO: maybe spring this instead of the sigmoid?
            var scaledADotB = (
                ia + iv
            );

            // sigmoid (thanks paradise)
            // https://www.desmos.com/calculator/stfjbdj5lh
            destIntensity = 1f + (2f / Mathf.PI) * Mathf.Atan(
                scaledADotB * Mathf.Pow(
                    Mathf.Abs(scaledADotB),
                    m_Steepness
                )
            );

            var sr = 0.08f;
            p += Vector3.right * 1.3f;
            DebugDraw.Push("dist-a",  p, sr * a * Vector3.up, new DebugDraw.Config(Color.BlanchedAlmond, count: 1));
            p += Vector3.right * 0.1f;
            DebugDraw.Push("dist-v",  p, sr * v * Vector3.up, new DebugDraw.Config(Color.Violet, count: 1));

            p += Vector3.right * 1.3f;
            var s = 1f;
            DebugDraw.Push("dist-ia",  p, s * ia * Vector3.up, new DebugDraw.Config(Color.BlanchedAlmond, count: 1));
            p += Vector3.right * 0.1f;
            DebugDraw.Push("dist-iv",  p, s * iv * Vector3.up, new DebugDraw.Config(Color.Violet, count: 1));
            p += Vector3.right * 0.1f;
            DebugDraw.Push("dist-if",  p, s * (scaledADotB) * Vector3.up, new DebugDraw.Config(Color.Yellow, count: 1));
        }

        // var x = 0f;
        // var speed = .1f;
        // var range = 5f;
        // // pos = transform.position;
        // DebugDraw.Push("target_ease", pos + x * Vector3.right + Vector3.up * m_Ease.Target.y, new DebugDraw.Config(Color.Tan));
        // DebugDraw.Push("value_ease", pos + x * Vector3.right, new DebugDraw.Config(Color.Violet));
        // x += speed * delta;
        // x = x % range;

        m_Ease.Update(delta, destIntensity * Vector3.up);

        p += Vector3.right * 1.3f;
        DebugDraw.Push("dist-dest",  p, m_Ease.Target, new DebugDraw.Config(Color.Red, count: 1));
        p += Vector3.right * 0.1f;
        DebugDraw.Push("dist-curr",  p, m_Ease.Value, new DebugDraw.Config(Color.Green, count: 1));
        DebugDraw.PushLine("dist-line",  p0, p, new DebugDraw.Config(Color.Black, count: 1));
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
                Mathf.Max(m_Ease.Value.y, 0f)
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