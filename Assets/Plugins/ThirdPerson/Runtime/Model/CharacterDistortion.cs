using Soil;
using UnityEngine;

namespace ThirdPerson {

sealed class CharacterDistortion: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the position for distorting from above the character")]
    [SerializeField] Vector3 m_Top;

    // -- props --
    /// .
    CharacterContainer c;

    /// the intensity ease on acceleration based stretch & squash
    DynamicEase<float> m_Ease;

    // -- lifecycle --
    void Start() {
        c = GetComponentInParent<CharacterContainer>();

        // initialize ease
        m_Ease.Init(1f, c.Tuning.Model.Distortion_Ease);
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
        if (c.State.Next.IsInJumpSquat) {
            var jumpId = c.State.Next.NextJump;
            var jumpSquatElapsed = c.State.Next.JumpState.PhaseElapsed;

            var jumpSquashIntensity = 0f;

            // use the squash curve if available
            var jumpTuning = tuning.JumpById(jumpId);
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
        var plane = new Plane(trs.up, trs.position).AsVector4();

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