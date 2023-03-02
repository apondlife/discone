using UnityEngine;

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

    /// the crouching animator prop
    const string k_PropIsCrouching = "IsCrouching";

    /// the move speed animator prop
    const string k_PropMoveSpeed = "MoveSpeed";

    /// the vertical speed animator prop
    const string k_PropVerticalSpeed = "VerticalSpeed";

    // -- fields --
    [Header("parameters")]
    [Tooltip("TODO: leave me a comment")]
    [SerializeField] bool AnimateScale;

    // -- stretch & squash --
    [Header("stretch & squash")]
    [Tooltip("TODO: leave me a comment")]
    [SerializeField] float MaxJumpSquatSquash;

    [Tooltip("TODO: leave me a comment")]
    [SerializeField] AnimationCurve JumpSquatSquashCurve;

    [Tooltip("TODO: leave me a comment")]
    [SerializeField] float VerticalAccelerationStretch;

    [Tooltip("TODO: leave me a comment")]
    [SerializeField] AnimationCurve VerticalAccelerationStretchCurve;

    [Tooltip("TODO: leave me a comment")]
    [SerializeField] float StretchAndSquashLerp;

    [Tooltip("TODO: leave me a comment")]
    [SerializeField] float MinSquashScale = 0;

    [Tooltip("TODO: leave me a comment")]
    [SerializeField] float MaxSquashScale = 2;

    // -- rotation --
    [Header("rotation")]
    [Tooltip("the body's rotation speed in degrees")]
    [SerializeField] float m_RotationSpeed = 0.0f;

    [Tooltip("the body's rotation away from the wall in degrees")]
    [SerializeField] float m_WallRotation = 30.0f;

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

    /// the character's tunables
    /// TODO: CharacterContainer, CharacterContainerConvertible
    CharacterTunablesBase m_Tunables => m_Container.Tunables;

    /// the character's tunables
    /// TODO: CharacterContainer, CharacterContainerConvertible
    CharacterInput m_Input => m_Container.Input;

    /// the list of ik limbs
    CharacterLimb[] m_Limbs;

    /// the current strech/squash multiplier
    float m_CurrentSquashStretch = 1.0f;

    /// the initial scale of the character
    Vector3 m_InitialScale;

    /// the legs layer index
    int m_LayerLegs;

    /// the arms layer index
    int m_LayerArms;

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
        }

        // make sure complex model trees have the correct layer
        SetDefaultLayersRecursively(gameObject, gameObject.layer);
    }

    void FixedUpdate() {
        // update animator & model
        SyncAnimator();
        Tilt();
        StretchAndSquash();
    }

    // -- commands --
    /// sync the animator's params
    void SyncAnimator() {
        var anim = m_Animator;
        if (anim == null || anim.runtimeAnimatorController == null) {
            return;
        }

        // set move animation params
        anim.SetFloat(
            k_PropMoveSpeed,
            Mathf.InverseLerp(
                0.0f,
                m_Tunables.Horizontal_MaxSpeed,
                m_State.Next.GroundVelocity.magnitude
            )
        );

        // set jump animation params
        anim.SetBool(
            k_PropIsAirborne,
            !m_State.Next.IsOnGround
        );

        anim.SetBool(
            k_PropIsCrouching,
            m_State.IsInJumpSquat || m_State.IsCrouching
        );

        anim.SetFloat(
            k_PropVerticalSpeed,
            m_State.Velocity.y
        );

        // blend yoshiing
        // TODO: lerp
        var yoshiing = !m_State.Next.IsOnGround && m_Input.IsJumpPressed ? 1.0f : 0.0f;
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
    void Tilt() {
        var rotation = m_State.Next.LookRotation;
        if (m_State.IsOnWall) {
            // rotation *= Quaternion.LookRotation(m_State.Forward, Vector3.up);
            rotation *= Quaternion.AngleAxis(m_WallRotation, m_State.Forward);
        }

        // is this a fundamental misunderstanding of quaternions? maybe
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            rotation,
            m_RotationSpeed * Time.deltaTime
        );

    }

    /// change character scale according to acceleration
    void StretchAndSquash() {
        if(!AnimateScale) {
            return;
        }

        var targetScale = 0.0f;
        if(m_State.IsInJumpSquat) {
            var jumpSquatPct = m_Tunables.Jumps[0].MaxJumpSquatFrames == 0 ? 1.0f : (float)m_State.JumpSquatFrame / m_Tunables.Jumps[0].MaxJumpSquatFrames;
            var jumpSquatDiff = 1.0f - MaxJumpSquatSquash;
            targetScale = (1.0f - jumpSquatDiff * (1.0f-JumpSquatSquashCurve.Evaluate(jumpSquatPct)));
        } else {
            // if accelerating against velocity, sigh should squash (negative sign), otherwise, stretch
            var sign = Mathf.Sign(m_State.Acceleration.y) * Mathf.Sign(m_State.Velocity.y);
            targetScale = (1.0f + sign * Mathf.Abs(m_State.Acceleration.y) * VerticalAccelerationStretch);
        }

        targetScale = Mathf.Clamp(targetScale, MinSquashScale, MaxSquashScale);
        m_CurrentSquashStretch = Mathf.Lerp(m_CurrentSquashStretch, targetScale, StretchAndSquashLerp * Time.deltaTime);

        var newScale = m_InitialScale;
        newScale.y *= m_CurrentSquashStretch;
        transform.localScale = newScale;
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