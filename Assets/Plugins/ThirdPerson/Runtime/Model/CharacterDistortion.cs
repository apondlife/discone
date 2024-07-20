using Soil;
using UnityEngine;

namespace ThirdPerson {

sealed class CharacterDistortion: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the top of the distortion field (e.g. the character's neck)")]
    [SerializeField] Transform m_Top;

    // -- props --
    /// .
    CharacterContainer c;

    /// the intensity ease on acceleration based stretch & squash
    DynamicEase<float> m_Ease;

    // -- lifecycle --
    void Start() {
        c = GetComponentInParent<CharacterContainer>();

        // initialize ease
        m_Ease = new DynamicEase<float>(c.Tuning.Model.Distortion_Ease);
        m_Ease.Init(1f);
    }

    void FixedUpdate() {
        var delta = Time.deltaTime;
        StretchAndSquash(delta);
        Distort();
    }

    /// change character scale according to acceleration
    void StretchAndSquash(float delta) {
        var tuning = c.Tuning.Model;

        var v = c.State.Prev.Velocity.y;
        var a = c.State.Curr.Acceleration.y * delta;

        // determine dest intensity
        var destIntensity = 1f;

        // if in jump squat, add jump squash
        var next = c.State.Next;
        if (next.IsInJumpSquat) {
            var jumpSquashIntensity = 0f;

            // use the squash curve if available
            var jumpTuning = tuning.JumpById(next.NextJump);
            if (jumpTuning != null) {
                jumpSquashIntensity = jumpTuning.Squash.Evaluate(next.JumpState.PhaseElapsed);
            }
            // otherwise, just use raw power
            else {
                jumpSquashIntensity = c.State.NextJumpPower;
            }

            destIntensity = jumpSquashIntensity;
        }
        // otherwise, stretch/squash based on vertical the acceleration/velocity relationship
        else {
            // if accelerating against velocity, sign should squash (negative sign), otherwise, stretch
            // TODO: maybe spring this instead of the sigmoid?
            var ia = tuning.Distortion_Intensity_Acceleration.Evaluate(Mathf.Sign(v * a) * 0.5f + 0.5f) * Mathf.Abs(a);
            var iv = tuning.Distortion_Intensity_Velocity.Evaluate(Mathf.Sign(v) * 0.5f + 0.5f) * Mathf.Abs(v);
            var intensity = ia + iv;

            // sigmoid (thanks paradise)
            // https://www.desmos.com/calculator/vrdxqcihwl
            destIntensity = 1f + (2f / Mathf.PI) * Mathf.Atan(
                tuning.Distortion_Movement_A * intensity *
                Mathf.Pow(
                    Mathf.Abs(tuning.Distortion_Movement_A * tuning.Distortion_Movement_B * intensity),
                    tuning.Distortion_Movement_K
                )
            );
        }

        m_Ease.Update(delta, destIntensity);
    }

    // -- commands --
    void Distort() {
        var trs = transform;
        var up = trs.up;

        var botPlane = new Plane(up, trs.position).AsVector4();
        var topPlane = new Plane(up, m_Top.position).AsVector4();

        foreach (var material in c.Model.Materials.All) {
            material.SetVector(
                ShaderProps.Character_Pos,
                c.State.Next.Position
            );

            material.SetVector(
                ShaderProps.Distortion_BotPlane,
                botPlane
            );

            material.SetVector(
                ShaderProps.Distortion_TopPlane,
                topPlane
            );

            material.SetFloat(
                ShaderProps.Distortion_Intensity,
                Mathf.Max(m_Ease.Value, 0f)
            );

            material.SetFloat(
                ShaderProps.Distortion_AxialScale,
                c.Tuning.Model.Distortion_AxialScale
            );

            material.SetFloat(
                ShaderProps.Distortion_RadialScale,
                c.Tuning.Model.Distortion_RadialScale
            );
        }
    }
}

}