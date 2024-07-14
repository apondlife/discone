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

    /// when the character is landing
    static readonly AnimatorProp s_IsLanding = new("IsLanding");

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

    // TODO: share this with other scripts that need the interpolated frame
    /// the interpolated state frame
    CharacterState.Frame m_Frame = new();

    // TODO: we should use a double total time here
    /// the stored last time of fixed update (for interpolation)
    float m_LastFixedUpdate = 0.0f;

    /// the current jumping leg (0-left, 1-right)
    int m_JumpLeg = 0;

    /// the landing timer
    EaseTimer m_IsLanding = new();

    /// the character's materials
    CharacterMaterials m_Materials;

    // -- deps --
    /// the containing character
    CharacterContainer c;

    // -- lifecycle --
    void Awake() {
        // init materials collection
        m_Materials = new CharacterMaterials(this);
    }

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
        var k = (Time.time - m_LastFixedUpdate) / Time.fixedDeltaTime;
        m_Frame.Interpolate(
            c.State.Curr,
            c.State.Next,
            k
        );

        // sync animator params
        SyncAnimator(m_Frame);
    }

    // -- commands --
    /// sync the animator's params
    void SyncAnimator(CharacterState.Frame frame) {
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
                frame.SurfaceVelocity.magnitude
            )
        );

        anim.SetFloat(
            s_MoveInputMag,
            c.Inputs.Move.magnitude
        );

        anim.SetFloat(
            s_MoveVelocityDotFacing,
            Vector3.Dot(frame.SurfaceVelocity, frame.Forward)
        );

        // set jump animation params
        var isJump = frame.Events.Contains(CharacterEvent.Jump);
        if (isJump) {
            anim.SetTrigger(s_Jump);
        }

        anim.SetBool(
            s_IsAirborne,
            frame.CoyoteTime <= 0
        );

        if (frame.IsInJumpSquat) {
            var jumpTuning = c.Tuning.NextJump(c.State);
            var jumpPower = jumpTuning.Power(frame.JumpState.PhaseElapsed);

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
            frame.IsInJumpSquat || frame.IsCrouching
        );

        anim.SetFloat(
            s_VerticalSpeed,
            frame.Velocity.y
        );

        if (c.State.Next.Events.Contains(CharacterEvent.Land)) {
            m_IsLanding.Duration = c.Tuning.Model.Animation_LandingDuration.Evaluate(c.State.Curr.Inertia);
            m_IsLanding.Start();

            // set landing props
            // TODO: fix rolling, fix pose blend-in
            anim.SetFloat(
                s_LandingSpeed,
                c.State.Curr.Inertia
            );
        }

        anim.SetBool(
            s_IsLanding,
            m_IsLanding.TryTick()
        );

        // blend yoshiing
        // TODO: lerp
        var surface = frame.PerceivedSurface;
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

    static void SetDefaultLayersRecursively(GameObject parent, int layer) {
        if (parent.layer == 0) {
            parent.layer = layer;
        }

        foreach(Transform child in parent.transform) {
            SetDefaultLayersRecursively(child.gameObject, layer);
        }
    }

    // -- queries --
    public CharacterMaterials Materials {
        get => m_Materials;
    }
}

}