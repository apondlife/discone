﻿using Soil;
using UnityEngine;

namespace ThirdPerson {

/// a container for the character's model and animations
public sealed class CharacterModel: MonoBehaviour {
    // -- constants --
    /// the legs animator layer (for yoshiing animation)
    const string k_LayerLegs = "Legs";

    /// the arms animator layer (for yoshiing animation)
    const string k_LayerArms = "Arms";

    /// when the character is airborne
    static readonly AnimatorProp s_IsAirborne = new("IsAirborne");

    /// when the character is crouching
    static readonly AnimatorProp s_IsCrouching = new("IsCrouching");

    /// when the character is landing
    static readonly AnimatorProp s_IsLanding = new("IsLanding");

    /// when the character is posing
    static readonly AnimatorProp s_IsPosing = new("IsPosing");

    /// the current move speed
    static readonly AnimatorProp s_MoveSpeed = new("MoveSpeed");

    /// the current move input
    static readonly AnimatorProp s_MoveInputMag = new("MoveInputMag");

    /// the dot product of move facing and velocity prop
    static readonly AnimatorProp s_MoveVelocityDotFacing = new("MoveVelocityDotFacing");

    /// the vertical speed
    static readonly AnimatorProp s_VerticalSpeed = new("VerticalSpeed");

    /// when the character jumps
    static readonly AnimatorProp s_Jump = new("Jump");

    /// the charge of the current jump
    static readonly AnimatorProp s_JumpCharge = new("JumpCharge");

    /// the current jump's travel distance
    static readonly AnimatorProp s_JumpDistance = new("JumpDistance");

    /// the index of the current jump's front leg
    static readonly AnimatorProp s_JumpLeg = new("JumpLeg");

    /// the character's inertia on landing
    static readonly AnimatorProp s_LandingSpeed = new("LandingSpeed");

    /// the scale of the current surface
    static readonly AnimatorProp s_SurfaceScale = new("SurfaceScale");

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

    /// the character's materials
    CharacterMaterials m_Materials;

    /// the legs layer index
    int m_LayerLegs;

    /// the arms layer index
    int m_LayerArms;

    // TODO: share this with other scripts that need the interpolated frame
    /// the interpolated state frame
    CharacterState.Frame m_Frame = new();

    /// the stored last time of fixed update (for interpolation)
    double m_FixedUpdateTime;

    /// the current jumping leg (0-left, 1-right)
    int m_JumpLeg;

    /// the start y-position of the current jump
    Vector3 m_JumpStartPos;

    /// the landing timer
    EaseTimer m_IsLanding = new();

    /// if the character is currently in the landing pose
    bool m_IsPosing;

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
        m_FixedUpdateTime = Time.timeAsDouble;

        // if the character jumped
        if (c.State.Next.Events.Contains(CharacterEvent.Jump)) {
            // alternate legs
            m_JumpLeg = (m_JumpLeg + 1) % 2;

            // and cache the initial y-position
            m_JumpStartPos = c.State.Next.Position;
        }
    }

    void Update() {
        // interpolate frame based on time since last update
        var k = (float)(Time.timeAsDouble - m_FixedUpdateTime) / Time.fixedDeltaTime;
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
        var moveSpeed = Mathx.InverseLerpUnclamped(
            0f,
            c.Tuning.Surface_MaxSpeed,
            frame.SurfaceVelocity.magnitude
        );

        anim.SetFloat(
            s_MoveSpeed,
            moveSpeed
        );

        anim.SetFloat(
            s_MoveInputMag,
            c.Inputs.Move.magnitude
        );

        anim.SetFloat(
            s_MoveVelocityDotFacing,
            Vector3.Dot(frame.SurfaceVelocity, frame.Forward)
        );

        anim.SetFloat(
            s_VerticalSpeed,
            frame.Velocity.y
        );

        // set jump squat params
        anim.SetBool(
            s_IsCrouching,
            frame.IsInJumpSquat
        );

        if (frame.IsInJumpSquat) {
            var jumpTuning = c.Tuning.NextJump(c.State);
            var jumpPower = jumpTuning.Power(frame.JumpState.PhaseElapsed);

            anim.SetFloat(
                s_JumpCharge,
                jumpPower
            );
        }

        // set jump animation params
        var isJump = frame.Events.Contains(CharacterEvent.Jump);
        if (isJump) {
            anim.SetTrigger(s_Jump);
        }

        anim.SetFloat(
            s_JumpLeg,
            m_JumpLeg
        );

        var isAirborne = frame.CoyoteTime <= 0f;
        anim.SetBool(
            s_IsAirborne,
            isAirborne
        );

        var jumpDist = 0f;
        if (isAirborne) {
            jumpDist = Vector3.Distance(m_JumpStartPos, frame.Position);

            anim.SetFloat(
                s_JumpDistance,
                jumpDist
            );
        }

        // set landing params
        if (c.State.Next.Events.Contains(CharacterEvent.Land)) {
            // start the timer
            m_IsLanding.Duration = c.Tuning.Model.Animation_LandingDuration.Evaluate(c.State.Curr.Inertia);
            m_IsLanding.Start();

            // see if we should pose
            m_IsPosing = (
                jumpDist > c.Tuning.Model.Animation_Pose_MinVerticalDistance &&
                moveSpeed < c.Tuning.Model.Animation_Pose_MaxMoveSpeed
            );

            // set landing props
            // TODO: fix rolling
            anim.SetFloat(
                s_LandingSpeed,
                c.State.Curr.Inertia
            );
        }

        if (m_IsPosing && moveSpeed > c.Tuning.Model.Animation_Pose_MaxMoveSpeed) {
            m_IsPosing = false;
        }

        anim.SetBool(
            s_IsLanding,
            m_IsLanding.TryTick() || m_IsPosing
        );

        anim.SetBool(
            s_IsPosing,
            m_IsPosing
        );

        // set surface params
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