using Soil;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {

/// a container for the character's model and animations
public sealed class CharacterModel: MonoBehaviour {
    // -- constants --
    /// the legs animator layer (for yoshiing animation)
    const string k_LayerLegs = "Legs";

    /// the arms animator layer (for yoshiing animation)
    const string k_LayerArms = "Arms";

    // TODO: Prop.SetXXX(x)
    /// the airborne animator prop
    const string k_PropIsAirborne = "IsAirborne";

    /// the jump charge animator prop
    const string k_PropJumpCharge = "JumpCharge";

    /// the is landing animator prop
    const string k_PropIsLanding = "IsLanding";

    /// the crouching animator prop
    const string k_PropIsCrouching = "IsCrouching";

    /// the move speed animator prop
    const string k_PropMoveSpeed = "MoveSpeed";

    /// the move input animator prop
    const string k_PropMoveInputMag = "MoveInputMag";

    /// the vertical speed animator prop
    const string k_PropVerticalSpeed = "VerticalSpeed";

    /// the landing speed animator prop
    const string k_PropLandingSpeed = "LandingSpeed";

    /// the dot product of move facing and velocity prop
    const string k_PropMoveFacingDotVelocity = "MoveFacingDotVelocity";

    /// the move input animator prop
    const string k_PropSurfaceScale = "SurfaceScale";

    /// the random value animator prop
    const string k_PropJumpLeg = "JumpLeg";

    // -- fields --
    [Header("config")]
    [Tooltip("the rotation speed in degrees towards look direction")]
    [FormerlySerializedAs("m_RotationSpeed_Look")]
    [FormerlySerializedAs("m_RotationSpeed")]
    [SerializeField] float m_LookRotation_Speed = 0.0f;

    [FormerlySerializedAs("m_RotationSpeed_Tilt")]
    [Tooltip("the rotation speed in degrees away for tilting")]
    [SerializeField] float m_MoveTilt_Speed = 100.0f;

    [FormerlySerializedAs("m_SurfaceRotation_Speed")]
    [FormerlySerializedAs("m_RotationSpeed_Wall")]
    [Tooltip("the rotation speed in degrees away from the wall")]
    [SerializeField] float m_SurfaceTilt_Speed = 0.0f;

    [Tooltip("the rotation away from the wall in degrees as a fn of surface angle")]
    [SerializeField] MapOutCurve m_SurfaceTilt_Range;

    [Tooltip("surface scaling factor as a function of surface angle (degrees)")]
    [SerializeField] AnimationCurve m_SurfaceScale;

    // -- refs --
    [Header("refs")]
    [Tooltip("the shared third person animator controller")]
    [SerializeField] RuntimeAnimatorController m_AnimatorController;

    // -- props --
    /// the character's animator
    Animator m_Animator;

    /// the containing character
    CharacterContainer m_Container;

    // TODO: CharacterContainer, CharacterContainerConvertible
    /// the character's state
    CharacterState m_State => m_Container.State;

    // TODO: CharacterContainer, CharacterContainerConvertible
    /// the character's tuning
    CharacterTuning m_Tuning => m_Container.Tuning;

    // TODO: CharacterContainer, CharacterContainerConvertible
    /// the character's tuning
    CharacterInputQuery m_Input => m_Container.InputQuery;

    /// the list of ik limbs
    CharacterLimb[] m_Limbs;

    /// the legs layer index
    int m_LayerLegs;

    /// the arms layer index
    int m_LayerArms;

    /// the stored look rotation
    Quaternion m_LookRotation = Quaternion.identity;

    /// the stored wall rotation
    Quaternion m_SurfaceTilt = Quaternion.identity;

    /// the stored tilt rotation
    Quaternion m_MoveTilt = Quaternion.identity;

    /// the stored speed when landing
    float m_LandingSpeed = 0.0f;

    /// the stored last time of fixedUpdate (for interpolation)
    float m_LastFixedUpdate = 0.0f;

    /// the current jumping leg (0-left, 1-right)
    int m_JumpLeg = 0;

    // -- lifecycle --
    void Start() {
        // set dependencies
        m_Container = GetComponentInParent<CharacterContainer>();

        // set props
        m_Limbs = GetComponentsInChildren<CharacterLimb>();

        // init animator
        m_Animator = GetComponentInChildren<Animator>();
        if (m_Animator != null) {
            if (m_Animator.runtimeAnimatorController == null) {
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

            // init ik limbs
            foreach (var limb in m_Limbs) {
                limb.Init(m_Animator);
            }

            // proxy animator callbacks
            var proxy = m_Animator.gameObject.GetComponent<CharacterAnimatorProxy>();
            if (proxy == null) {
                proxy = m_Animator.gameObject.AddComponent<CharacterAnimatorProxy>();
            }

            proxy.Bind(OnAnimatorIK);
        } else {
            // destroy ik limbs
            Log.Model.W($"character {m_Container.Name} has no animator, destroying limbs");
            foreach (Component limb in m_Limbs) {
                Destroy(limb.gameObject);
            }
        }

        // make sure complex model trees have the correct layer
        SetDefaultLayersRecursively(gameObject, gameObject.layer);
    }

    void FixedUpdate() {
        m_LastFixedUpdate = Time.time;

        // alternate legs
        if (m_State.Next.Events.Contains(CharacterEvent.Jump)) {
            m_JumpLeg = (m_JumpLeg + 1) % 2;
        }
    }

    void Update() {
        // interpolate frame based on time since last update
        var delta = Time.time - m_LastFixedUpdate;
        var state = CharacterState.Frame.Interpolate(
            m_State.Curr,
            m_State.Next,
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
        if (anim == null || anim.runtimeAnimatorController == null) {
            return;
        }

        // set move animation params
        anim.SetFloat(
            k_PropMoveSpeed,
            Mathx.InverseLerpUnclamped(
                0.0f,
                m_Tuning.Surface_MaxSpeed,
                state.SurfaceVelocity.magnitude
            )
        );

        anim.SetFloat(
            k_PropMoveInputMag,
            m_Input.Move.magnitude
        );

        // set jump animation params
        anim.SetBool(
            k_PropIsLanding,
            state.IsLanding
        );

        anim.SetBool(
            k_PropIsAirborne,
            state.MainSurface.IsNone
        );


        if (state.IsInJumpSquat) {
            var jumpSquatPct = 1f;
            var jumpTuning = m_Tuning.Jumps[m_State.JumpTuningIndex];
            if (jumpTuning.JumpSquatDuration.Max > 0f) {
                jumpSquatPct = state.JumpState.PhaseElapsed / jumpTuning.JumpSquatDuration.Max;
            }

            anim.SetFloat(
                k_PropJumpLeg,
                m_JumpLeg
            );

            anim.SetFloat(
                k_PropJumpCharge,
                jumpSquatPct
            );
        }

        anim.SetBool(
            k_PropIsCrouching,
            state.IsInJumpSquat || state.IsCrouching
        );

        anim.SetFloat(
            k_PropVerticalSpeed,
            state.Velocity.y
        );

        // TODO: fix rolling
        m_LandingSpeed = state.IsColliding ? state.Inertia : 0f;
        anim.SetFloat(
            k_PropLandingSpeed,
            m_LandingSpeed
        );

        anim.SetFloat(
            k_PropMoveFacingDotVelocity,
            Vector3.Dot(state.SurfaceVelocity.normalized, state.Forward)
        );

        // blend yoshiing
        // TODO: lerp
        var surface = state.MainSurface;
        var yoshiing = (m_Input.IsJumpPressed ? 1.0f : 0.0f);

        if (surface.IsSome) {
            yoshiing *= Mathf.Abs(surface.Angle / 90.0f);

            anim.SetFloat(
                k_PropSurfaceScale,
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

    /// a callback for calculating IK
    void OnAnimatorIK(int layer) {
        foreach (var limb in m_Limbs) {
            limb.ApplyIk();
        }
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
            surfaceTiltTangent
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

        m_LookRotation = Quaternion.RotateTowards(
            m_LookRotation,
            state.LookRotation,
            m_LookRotation_Speed * delta
        );

        transform.localRotation = m_SurfaceTilt * m_MoveTilt * m_LookRotation;
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