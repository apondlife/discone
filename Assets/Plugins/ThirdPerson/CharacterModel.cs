using UnityEngine;

namespace ThirdPerson {

/// a container for the character's model and animations
public sealed class CharacterModel: MonoBehaviour {
    // -- fields --
    [Header("parameters")]
    [SerializeField] private bool AnimateScale;
    [SerializeField] private float MaxJumpSquatSquash;
    [SerializeField] private AnimationCurve JumpSquatSquashCurve;
    [SerializeField] private float VerticalAccelerationStretch;
    [SerializeField] private AnimationCurve VerticalAccelerationStretchCurve;
    [SerializeField] private float StretchAndSquashLerp;
    [SerializeField] private float MinSquashScale = 0;
    [SerializeField] private float MaxSquashScale = 2;

    [Header("references")]
    [Tooltip("The shared third person animator controller")]
    [SerializeField] private RuntimeAnimatorController m_AnimatorController;

    // -- props --
    /// the character's animator
    private Animator m_Animator;

    /// the character's state
    private CharacterState m_State;

    /// the character's tunables
    private CharacterTunablesBase m_Tunables;

    /// the initial scale of the character
    private Vector3 c_BaseScale;

    /// the current strech/squash multiplier
    private float m_CurrentSquashStretch = 1.0f;

    // -- lifecycle --
    void Start() {
        // set dependencies
        var character = GetComponentInParent<Character>();
        m_State = character.State;
        m_Tunables = character.Tunables;

        // configure animator
        m_Animator = GetComponentInChildren<Animator>();
        if (m_Animator != null && m_Animator.runtimeAnimatorController == null) {
            m_Animator.runtimeAnimatorController = m_AnimatorController;
        }

        c_BaseScale = transform.localScale;

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
            "MoveSpeed",
            Mathf.InverseLerp(
                m_Tunables.MinPlanarSpeed,
                m_Tunables.MaxPlanarSpeed,
                m_State.PlanarVelocity.magnitude
            )
        );

        // set jump animation params
        anim.SetBool(
            "JumpSquat",
            m_State.IsInJumpSquat
        );

        anim.SetBool(
            "Airborne",
            !m_State.IsGrounded
        );

        anim.SetFloat(
            "VerticalSpeed",
            m_State.VerticalSpeed
        );
    }

    /// tilt the model as a fn of character acceleration
    void Tilt() {
        // is this a fundamental misunderstanding of quaternions? maybe
        transform.rotation = m_State.LookRotation;
    }

    void StretchAndSquash() {
        if(!AnimateScale) {
            return;
        }

        var targetScale = 0.0f;
        if(m_State.IsInJumpSquat) {
            var jumpSquatPct = m_Tunables.MaxJumpSquatFrames == 0 ? 1.0f : (float)m_State.JumpSquatFrame / m_Tunables.MaxJumpSquatFrames;
            var jumpSquatDiff = 1.0f - MaxJumpSquatSquash;
            targetScale = (1.0f - jumpSquatDiff * (1.0f-JumpSquatSquashCurve.Evaluate(jumpSquatPct)));
        } else {
            // if accelerating against velocity, sigh should squash (negative sign), otherwise, stretch
            var sign = Mathf.Sign(m_State.Acceleration.y) * Mathf.Sign(m_State.Velocity.y);
            targetScale = (1.0f + sign * Mathf.Abs(m_State.Acceleration.y) * VerticalAccelerationStretch);
        }

        m_CurrentSquashStretch = Mathf.Lerp(m_CurrentSquashStretch, targetScale, StretchAndSquashLerp);
        m_CurrentSquashStretch = Mathf.Clamp(m_CurrentSquashStretch, MinSquashScale, MaxSquashScale);

        var newScale = c_BaseScale;
        newScale.y *= m_CurrentSquashStretch;
        transform.localScale = newScale;
    }
}

}