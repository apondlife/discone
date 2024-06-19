using Soil;
using UnityEngine;

namespace ThirdPerson {

/// a container for the character's model and animations
public sealed class CharacterModel: MonoBehaviour {
    // -- constants --
    /// the legs animator layer (for yoshiing animation)
    const string k_LayerLegs = "Legs";

    /// the arms animator layer (for yoshiing animation)
    const string k_LayerArms = "Arms";

    /// the move speed animator prop
    static readonly AnimatorProp s_MoveSpeed = new("MoveSpeed");

    /// the move input animator prop
    static readonly AnimatorProp s_MoveInputMag = new("MoveInputMag");

    /// the dot product of move facing and velocity prop
    static readonly AnimatorProp s_MoveVelocityDotFacing = new("MoveVelocityDotFacing");

    /// the move input animator prop
    static readonly AnimatorProp s_SurfaceScale = new("SurfaceScale");

    /// the landing speed animator prop
    static readonly AnimatorProp s_LandingSpeed = new("LandingSpeed");

    /// the airborne jump trigger prop
    static readonly AnimatorProp s_Jump = new("Jump");

    /// the jump charge animator prop
    static readonly AnimatorProp s_JumpCharge = new("JumpCharge");

    /// the random value animator prop
    static readonly AnimatorProp s_JumpLeg = new("JumpLeg");

    /// the vertical speed animator prop
    static readonly AnimatorProp s_VerticalSpeed = new("VerticalSpeed");

    /// the airborne animator prop
    static readonly AnimatorProp s_IsAirborne = new("IsAirborne");

    /// the crouching animator prop
    static readonly AnimatorProp s_IsCrouching = new("IsCrouching");

    // -- tuning --
    [Header("tuning")]
    [Tooltip("surface scaling factor as a function of surface angle (degrees)")]
    [SerializeField] AnimationCurve m_SurfaceScale;

    // -- tuning/tilt --
    [Header("tuning/tilt")]
    [Tooltip("the rotation speed in degrees away for tilting")]
    [SerializeField] float m_MoveTilt_Speed = 100.0f;

    [Tooltip("the rotation speed in degrees away from the wall")]
    [SerializeField] float m_SurfaceTilt_Speed = 0.0f;

    [Tooltip("the rotation away from the wall in degrees as a fn of surface angle")]
    [SerializeField] MapOutCurve m_SurfaceTilt_Range;

    // -- refs --
    [Header("refs")]
    [Tooltip("the shared third person animator controller")]
    [SerializeField] RuntimeAnimatorController m_AnimatorController;

    // -- props --
    /// the character's animator
    Animator m_Animator;

    /// the legs layer index
    int m_LayerLegs;

    /// the arms layer index
    int m_LayerArms;

    /// the stored wall rotation
    Quaternion m_SurfaceTilt = Quaternion.identity;

    /// the stored tilt rotation
    Quaternion m_MoveTilt = Quaternion.identity;

    /// the stored last time of fixed update (for interpolation)
    float m_LastFixedUpdate = 0.0f;

    /// the current jumping leg (0-left, 1-right)
    int m_JumpLeg = 0;

    // -- deps --
    /// the containing character
    CharacterContainer c;

    // -- lifecycle --
    void Start() {
        // set dependencies
        c = GetComponentInParent<CharacterContainer>();

        // init animator
        m_Animator = GetComponentInChildren<Animator>();
        if (m_Animator) {
            if (!m_Animator.runtimeAnimatorController) {
                m_Animator.runtimeAnimatorController = m_AnimatorController;
            }

            // disable root motion
            if (m_Animator.applyRootMotion) {
                Log.Model.W($"disabled animator root motion, make sure to uncheck this in animator");
                m_Animator.applyRootMotion = false;
            }

            // set layers indices
            m_LayerLegs = m_Animator.GetLayerIndex(k_LayerLegs);
            m_LayerArms = m_Animator.GetLayerIndex(k_LayerArms);
        }

        // make sure complex model trees have the correct layer
        SetDefaultLayersRecursively(gameObject, gameObject.layer);
    }

    void FixedUpdate() {
        m_LastFixedUpdate = Time.time;

        // alternate legs
        if (c.State.Next.Events.Contains(CharacterEvent.Jump)) {
            m_JumpLeg = (m_JumpLeg + 1) % 2;
        }
    }

    void Update() {
        // interpolate frame based on time since last update
        var delta = Time.time - m_LastFixedUpdate;
        var state = CharacterState.Frame.Interpolate(
            c.State.Curr,
            c.State.Next,
            delta / Time.fixedDeltaTime
        );

        // update animator & model
        SyncAnimator(state);
        Tilt(state, Time.deltaTime);
    }

    // -- commands --
    /// sync the animator's params
    void SyncAnimator(CharacterState.Frame state) {
        var anim = m_Animator;
        if (!anim || !anim.runtimeAnimatorController) {
            return;
        }

        // set move animation params
        anim.SetFloat(
            s_MoveSpeed,
            Mathx.InverseLerpUnclamped(
                0.0f,
                c.Tuning.Surface_MaxSpeed,
                state.SurfaceVelocity.magnitude
            )
        );

        anim.SetFloat(
            s_MoveInputMag,
            c.Inputs.Move.magnitude
        );

        // set jump animation params
        var isJump = state.Events.Contains(CharacterEvent.Jump);
        if (isJump) {
            anim.SetTrigger(s_Jump);
        }

        anim.SetBool(
            s_IsAirborne,
            state.MainSurface.IsNone || isJump
        );

        if (state.IsInJumpSquat) {
            var jumpTuning = c.Tuning.NextJump(c.State);
            var jumpPower = jumpTuning.Power(state.JumpState.PhaseElapsed);

            anim.SetFloat(
                s_JumpLeg,
                m_JumpLeg
            );

            anim.SetFloat(
                s_JumpCharge,
                jumpPower
            );
        }

        anim.SetBool(
            s_IsCrouching,
            state.IsInJumpSquat || state.IsCrouching
        );

        anim.SetFloat(
            s_VerticalSpeed,
            state.Velocity.y
        );

        // TODO: fix rolling
        anim.SetFloat(
            s_LandingSpeed,
            state.IsColliding ? state.Inertia : 0f
        );

        anim.SetFloat(
            s_MoveVelocityDotFacing,
            Vector3.Dot(state.SurfaceVelocity, state.Forward)
        );

        // blend yoshiing
        // TODO: lerp
        var surface = state.MainSurface;
        var yoshiing = (c.Inputs.IsJumpPressed ? 1.0f : 0.0f);

        if (surface.IsSome) {
            yoshiing *= Mathf.Abs(surface.Angle / 90.0f);

            anim.SetFloat(
                s_SurfaceScale,
                m_SurfaceScale.Evaluate(surface.Angle)
            );
        }

        // layers
        anim.SetLayerWeight(
            m_LayerLegs,
            yoshiing
        );

        anim.SetLayerWeight(
            m_LayerArms,
            yoshiing
        );
    }

    /// tilt the model as a fn of character acceleration
    void Tilt(CharacterState.Frame state, float delta) {
        var surface = state.MainSurface;
        var surfaceTiltTangent = Vector3.Cross(
            Vector3.up,
            surface.Normal
        );

        var destSurfaceTilt = Quaternion.AngleAxis(
            m_SurfaceTilt_Range.Evaluate(surface.Angle),
            transform.InverseTransformDirection(surfaceTiltTangent)
        );

        m_SurfaceTilt = Quaternion.RotateTowards(
            m_SurfaceTilt,
            destSurfaceTilt,
            m_SurfaceTilt_Speed * delta
        );

        m_MoveTilt = Quaternion.RotateTowards(
            m_MoveTilt,
            state.Tilt,
            m_MoveTilt_Speed * delta
        );

        transform.localRotation = m_SurfaceTilt * m_MoveTilt;
    }

    static void SetDefaultLayersRecursively(GameObject parent, int layer) {
        if (parent.layer == 0) {
            parent.layer = layer;
        }

        foreach(Transform child in parent.transform) {
            SetDefaultLayersRecursively(child.gameObject, layer);
        }
    }
}

}