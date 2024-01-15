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

    /// the airborne animator prop
    const string k_PropIsAirborne = "IsAirborne";

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

    // -- fields --
    [Header("parameters")]
    [Tooltip("TODO: leave me a comment")]
    [SerializeField] bool AnimateScale;

    // -- rotation --
    [Header("rotation")]
    [Tooltip("the rotation speed in degrees towards look direction")]
    [FormerlySerializedAs("m_RotationSpeed")]
    [SerializeField] float m_RotationSpeed_Look = 0.0f;

    [Tooltip("the rotation speed in degrees away from the wall")]
    [SerializeField] float m_RotationSpeed_Wall = 0.0f;

    [Tooltip("the rotation speed in degrees away for tilting")]
    [SerializeField] float m_RotationSpeed_Tilt = 100.0f;

    [Tooltip("the rotation away from the wall in degrees")]
    [FormerlySerializedAs("m_WallRotation")]
    [SerializeField] float m_MaxWallRotation = 30.0f;

    // -- refs --
    [Header("refs")]
    [Tooltip("the shared third person animator controller")]
    [SerializeField] RuntimeAnimatorController m_AnimatorController;

    // -- props --
    /// the character's animator
    Animator m_Animator;

    /// the containing character
    Character m_Container;

    /// the character's state
    /// TODO: CharacterContainer, CharacterContainerConvertible
    CharacterState m_State => m_Container.State;

    /// the character's tuning
    /// TODO: CharacterContainer, CharacterContainerConvertible
    CharacterTuning m_Tuning => m_Container.Tuning;

    /// the character's tuning
    /// TODO: CharacterContainer, CharacterContainerConvertible
    CharacterInput m_Input => m_Container.Input;

    /// the list of ik limbs
    CharacterLimb[] m_Limbs;

    /// the initial scale of the character
    Vector3 m_InitialScale;

    /// the legs layer index
    int m_LayerLegs;

    /// the arms layer index
    int m_LayerArms;

    /// the stored look rotation
    Quaternion m_LookRotation = Quaternion.identity;

    /// the stored wall rotation
    Quaternion m_WallRotation = Quaternion.identity;

    /// the stored tilt rotation
    Quaternion m_TiltRotation = Quaternion.identity;

    /// the stored speed when landing
    float m_LandingSpeed = 0.0f;

    /// the stored last time of fixedUpdate (for interpolation)
    float m_LastFixedUpdate = 0.0f;

    // -- lifecycle --
    void Start() {
        // set dependencies
        m_Container = GetComponentInParent<Character>();

        // set props
        m_Limbs = GetComponentsInChildren<CharacterLimb>();
        m_InitialScale = transform.localScale;

        // init animator
        m_Animator = GetComponentInChildren<Animator>();
        if (m_Animator != null) {
            if (m_Animator.runtimeAnimatorController == null) {
                m_Animator.runtimeAnimatorController = m_AnimatorController;
            }

            // disable root motion
            if (m_Animator.applyRootMotion) {
                Debug.LogWarning("[cmodel] disabled animator root motion, make sure to uncheck this in animator");
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
            Debug.LogWarning($"[cmodel] character {m_Container.Name} has no animator, destroying limbs");
            foreach (Component limb in m_Limbs) {
                Destroy(limb.gameObject);
            }
        }

        // make sure complex model trees have the correct layer
        SetDefaultLayersRecursively(gameObject, gameObject.layer);
    }

    void FixedUpdate() {
        m_LastFixedUpdate = Time.time;
    }

    void Update() {
        // interpolate frame based on time since last update
        var end = m_State.Next;
        var delta = Time.time - m_LastFixedUpdate;
        var state = CharacterState.Frame.Interpolate(m_State.Curr, end,  delta / Time.fixedDeltaTime);

        // update animator & model
        SyncAnimator(state);
        Tilt(state);
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
                m_Tuning.Horizontal_MaxSpeed,
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

        var isAirborne = state.MainSurface.IsNone || state.MainSurface.Angle > m_Tuning.Jump_GroundAngle;
        anim.SetBool(
            k_PropIsAirborne,
            isAirborne
        );

        anim.SetBool(
            k_PropIsCrouching,
            state.IsInJumpSquat || state.IsCrouching
        );

        anim.SetFloat(
            k_PropVerticalSpeed,
            state.Velocity.y
        );


        if (!state.IsOnGround) {
            m_LandingSpeed = state.Velocity.y;
        }

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
        var yoshiing = state.MainSurface.IsNone && m_Input.IsJumpPressed ? 1.0f : 0.0f;
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
    void Tilt(CharacterState.Frame state) {
        var destWallRotation = Quaternion.identity;
        if (state.IsOnWall && !state.IsOnGround) {
            var tangent = Vector3.Cross(Vector3.up, state.MainSurface.Normal);
            destWallRotation = Quaternion.AngleAxis(m_MaxWallRotation, tangent);
        }

        m_WallRotation = Quaternion.RotateTowards(
            m_WallRotation,
            destWallRotation,
            m_RotationSpeed_Wall * Time.deltaTime
        );

        m_TiltRotation = Quaternion.RotateTowards(
            m_TiltRotation,
            state.Tilt,
            m_RotationSpeed_Tilt * Time.deltaTime
        );

        m_LookRotation = Quaternion.RotateTowards(
            m_LookRotation,
            state.LookRotation,
            m_RotationSpeed_Look * Time.deltaTime
        );

        transform.localRotation = m_WallRotation * m_TiltRotation * m_LookRotation;
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