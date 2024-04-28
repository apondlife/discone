using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {

// TODO: center of mass? move character down?
/// an ik limb for the character model
public partial class Limb: MonoBehaviour, CharacterPart, LimbAnchor, LimbContainer {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the type of goal of this limb")]
    [SerializeField] AvatarIKGoal m_Goal;

    [Tooltip("the tuning")]
    [SerializeField] LimbTuning m_Tuning;

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

    /// the initial distance to the goal
    float m_LimbLen;

    /// the offset of the end bone used for placement, if any
    float m_EndLen;

    /// the position of the ik goal
    Vector3 m_GoalPos;

    /// the rotation of the ik goal
    Quaternion m_GoalRot;

    // -- lifecycle --
    void Update() {
        if (!IsValid) {
            return;
        }

        var delta = Time.deltaTime;
        m_StrideSystem.Update(delta);

        // set target ik weight if limb is active
        var weight = m_StrideSystem.IsActive ? 1f : 0f;

        // interpolate the weight
        var blendSpeed = weight > m_Weight ? m_Tuning.Blend_InSpeed : m_Tuning.Blend_OutSpeed;
        weight = Mathf.MoveTowards(
            m_Weight,
            weight,
            blendSpeed * delta
        );

        var normal = m_StrideSystem.Normal;

        // get next position goal position, removing the end offset if necessary
        var goalPos = m_StrideSystem.GoalPos;
        if (normal != Vector3.zero) {
            goalPos += m_EndLen * normal;
        }

        // if not held, interpolate position.
        if (!m_StrideSystem.IsHeld) {
            goalPos = Vector3.MoveTowards(
                m_GoalPos,
                goalPos,
                m_Tuning.Goal_MoveSpeed * Time.deltaTime
            );
        }

        // get next rotation
        var up = normal;

        // if no normal, use the direction towards the root
        if (up == Vector3.zero) {
            up = Vector3.Normalize(transform.position - goalPos);
        }

        var goalRot = Quaternion.LookRotation(
            Vector3.ProjectOnPlane(c.State.Curr.Forward, up),
            up
        );

        // always interpolate rotation
        goalRot = Quaternion.RotateTowards(
            m_GoalRot,
            goalRot,
            m_Tuning.Goal_RotationSpeed * Time.deltaTime
        );

        // update state
        m_Weight = weight;
        m_GoalPos = goalPos;
        m_GoalRot = goalRot;

        // TODO: consider how to compile out debug utils; DEVELOPMENT_BUILD, DEBUG?
        Debug_Update();
    }

    // -- commands --
    /// initialize this limb w/ an animator
    public void Init(Animator animator) {
        // set deps
        c = GetComponentInParent<CharacterContainer>();

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

        m_LimbLen = limb.magnitude;
        m_EndLen = endLength;

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
    public void Move(LimbAnchor anchor) {
        m_StrideSystem.Move(anchor);
    }

    /// holds the limb if not already
    public void Hold(float delta) {
        m_StrideSystem.Hold(delta);
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
            m_Animator.SetIKPosition(
                m_Goal,
                m_GoalPos
            );

            m_Animator.SetIKRotation(
                m_Goal,
                m_GoalRot
            );
        }

        Debug_ApplyIk();
    }

    // -- queries --
    /// if this limb has the dependencies it needs to apply ik
    bool IsValid {
        get => m_GoalBone;
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

    /// cast for the distance to the nearest surface in the limb direction
    public float HeldDistance {
        get => m_StrideSystem.HeldDistance;
    }

    // -- CharacterLimbContainer --
    /// the ik goal
    public AvatarIKGoal Goal {
        get => m_Goal;
    }

    /// the current root bone position
    public Vector3 RootPos {
        get => transform.position;
    }

    /// the tuning for the limb
    public LimbTuning Tuning {
        get => m_Tuning;
    }

    /// the character container
    public CharacterContainer Character {
        get => c;
    }

    /// the bone the stride is anchored by
    public LimbAnchor InitialAnchor {
        get => this;
    }

    /// the length of the limb
    public float InitialLen {
        get => m_LimbLen + m_EndLen;
    }

    /// the direction towards the surface
    public Vector3 InitialDir {
        get => transform.forward;
    }
}

}