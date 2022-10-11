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

    // -- refs --
    [Header("refs")]
    [Tooltip("The shared third person animator controller")]
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

        // configure animator
        m_Animator = GetComponentInChildren<Animator>();
        if (m_Animator != null && m_Animator.runtimeAnimatorController == null) {
            m_Animator.runtimeAnimatorController = m_AnimatorController;
        }

        // set props
        m_InitialScale = transform.localScale;
        m_LayerLegs = m_Animator.GetLayerIndex(k_LayerLegs);
        m_LayerArms = m_Animator.GetLayerIndex(k_LayerArms);

        // make sure complex model trees have the correct layer
        gameObject.SetLayerRecursively(gameObject.layer);
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
                m_Tunables.Horizontal_MinSpeed,
                m_Tunables.Horizontal_MaxSpeed,
                m_State.Curr.GroundVelocity.magnitude
            )
        );

        anim.SetBool(
            k_PropIsCrouching,
            m_State.IsInJumpSquat || m_State.IsCrouching
        );

        // set jump animation params
        anim.SetBool(
            k_PropIsAirborne,
            !m_State.IsGrounded
        );

        anim.SetFloat(
            k_PropVerticalSpeed,
            m_State.Velocity.y
        );

        // blend yoshiing
        // TODO: lerp
        var yoshiing = !m_State.IsGrounded && m_Input.IsJumpPressed ? 1.0f : 0.0f;
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
    void Tilt() {
        // is this a fundamental misunderstanding of quaternions? maybe
        transform.rotation = m_State.Curr.LookRotation;
    }

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
}

}