using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {

// TODO: center of mass? move character down?
/// an ik limb for the character model
public partial class CharacterLimb: MonoBehaviour, CharacterPart, CharacterLimbAnchor, LimbContainer {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the type of goal of this limb")]
    [SerializeField] AvatarIKGoal m_Goal;

    // -- tuning --
    [Header("tuning")]
    [Tooltip("the extra velocity when blending ik as a function of input")]
    [SerializeField] float m_Blend_InputVelocity;

    [FormerlySerializedAs("m_BlendInDuration")]
    [Tooltip("the speed the ik weight blends towards one")]
    [SerializeField] float m_Blend_InSpeed;

    [FormerlySerializedAs("m_BlendOutDuration")]
    [Tooltip("the speed the ik weight blends towards zero")]
    [SerializeField] float m_Blend_OutSpeed;

    // -- systems --
    [Header("systems")]
    [Tooltip("the limb system")]
    [SerializeField] StrideSystem m_StrideSystem;

    // -- props --
    /// the containing character
    CharacterContainer c;

    /// the animator
    Animator m_Animator;

    /// the transform of the goal bone, if any
    Transform m_GoalBone;

    /// the current blend weight
    float m_Weight;

    /// the length of the limb
    float m_InitialLen;

    /// the offset of the end bone used for placement, if any
    float m_EndLength;

    // -- lifecycle --
    void Awake() {
        // set deps
        c = GetComponentInParent<CharacterContainer>();
    }

    void Update() {
        if (!IsValid) {
            return;
        }

        var delta = Time.deltaTime;
        m_StrideSystem.Update(delta);

        // set target ik weight if limb is active
        var destWeight = m_StrideSystem.IsActive ? 1f : 0f;

        // interpolate the weight
        var blendSpeed = destWeight > m_Weight ? m_Blend_InSpeed : m_Blend_OutSpeed;
        m_Weight = Mathf.MoveTowards(
            m_Weight,
            destWeight,
            blendSpeed * delta
        );

        // TODO: consider how to compile out debug utils; DEVELOPMENT_BUILD, DEBUG?
        Debug_Update();
    }

    // -- commands --
    /// initialize this limb w/ an animator
    public void Init(Animator animator) {
        // set props
        m_Animator = animator;

        // cache the bone; we can't really do anything if we don't find a bone
        m_GoalBone = m_Animator.GetBoneTransform(
            m_Goal switch {
                AvatarIKGoal.RightHand => HumanBodyBones.RightHand,
                AvatarIKGoal.LeftHand => HumanBodyBones.LeftHand,
                AvatarIKGoal.RightFoot => HumanBodyBones.RightFoot,
                AvatarIKGoal.LeftFoot => HumanBodyBones.LeftFoot,
                _ => throw new Exception($"invalid goal {m_Goal}")
            }
        );

        var lastBone = m_Animator.GetBoneTransform(
            m_Goal switch {
                AvatarIKGoal.RightHand => HumanBodyBones.RightHand,
                AvatarIKGoal.LeftHand => HumanBodyBones.LeftHand,
                AvatarIKGoal.RightFoot => HumanBodyBones.RightToes,
                AvatarIKGoal.LeftFoot => HumanBodyBones.LeftToes,
                _ => throw new Exception($"invalid goal {m_Goal}")
            }
        );

        var endBone = lastBone ? lastBone.GetChild(0) : null;

        if (!m_GoalBone || !endBone) {
            Log.Model.E($"{c.Name} - <limb: {m_Goal}> no matching bone");
            return;
        }

        // get default limb lengths
        var goalPos = m_GoalBone.position;

        var limb = goalPos - transform.position;
        var limbDir = limb.normalized;

        var end = endBone.position - goalPos;
        var endLength = Vector3.Dot(end, limbDir);

        m_InitialLen = limb.magnitude + endLength;
        m_EndLength = endLength;

        // align root direction
        transform.forward = limbDir;

        // init system
        m_StrideSystem.Init(this);
    }

    /// toggles stride system enabled
    public void SetIsStriding(bool isStriding) {
        m_StrideSystem.SetIsStriding(isStriding);
    }

    /// starts a new stride for the limb
    public void Move(CharacterLimbAnchor anchor) {
        m_StrideSystem.Move(anchor);
    }

    /// released the limb's hold if not already
    public void Release() {
        m_StrideSystem.Release();
    }

    /// set the slide offset to translate the legs
    public void SetSlideOffset(Vector3 offset) {
        m_StrideSystem.SetSlideOffset(offset);
    }

    /// applies the limb ik
    public void ApplyIk() {
        if (!IsValid) {
            return;
        }

        m_Animator.SetIKPositionWeight(
            m_Goal,
            m_Weight
        );

        m_Animator.SetIKRotationWeight(
            m_Goal,
            m_Weight
        );

        if (m_Weight != 0.0f) {
            var hitUp = m_StrideSystem.Normal;

            var goalPos = m_StrideSystem.GoalPos;
            if (hitUp != Vector3.zero) {
                goalPos += m_EndLength * hitUp;
            }

            m_Animator.SetIKPosition(
                m_Goal,
                goalPos
            );

            var up = hitUp;
            if (up == Vector3.zero) {
                up = Vector3.Normalize(transform.position - goalPos);
            }

            var rot = Quaternion.LookRotation(
                Vector3.ProjectOnPlane(c.State.Curr.Forward, up),
                up
            );

            m_Animator.SetIKRotation(
                m_Goal,
                rot
            );
        }
    }

    // -- queries --
    /// if this limb has the dependencies it needs to apply ik
    bool IsValid {
        get => m_GoalBone;
    }

    /// the current root bone position
    public Vector3 RootPos {
        get => transform.position;
    }

    /// the current goal bone position
    public Vector3 GoalPos {
        get => m_StrideSystem.GoalPos;
    }

    /// .
    public bool IsFree {
        get => m_StrideSystem.IsFree;
    }

    /// .
    public bool IsHeld {
        get => m_StrideSystem.IsHeld;
    }

    // -- LimbContainer --
    /// the ik goal
    public AvatarIKGoal Goal {
        get => m_Goal;
    }

    /// the bone the stride is anchored by
    public CharacterLimbAnchor InitialAnchor {
        get => this;
    }

    /// the length of the limb
    public float InitialLen {
        get => m_InitialLen;
    }

    /// the direction towards the surface
    public Vector3 InitialDir {
        get => transform.forward;
    }

    /// the character container
    public CharacterContainer Character {
        get => c;
    }

}

}