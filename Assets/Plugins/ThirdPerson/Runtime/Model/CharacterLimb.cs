using System;
using Soil;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {

/// center of mass? move character down?
/// an ik limb for the character model
public class CharacterLimb: MonoBehaviour, CharacterPart, CharacterBone {
    // -- deps --
    /// the containing character
    CharacterContainer c;

    /// the animator for this limb
    Animator m_Animator;

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the type of goal of this limb")]
    [SerializeField] AvatarIKGoal m_Goal;

    // -- tuning --
    [Header("tuning")]
    [Tooltip("the duration of the ik blend when searching for target")]
    [SerializeField] float m_BlendInDuration;

    [Tooltip("the duration of the ik blend when dropping target")]
    [SerializeField] float m_BlendOutDuration;

    [FormerlySerializedAs("m_System")]
    [Tooltip("the limb system")]
    [SerializeField] StrideSystem m_StrideSystem;

    // -- props --
    /// the transform of the goal bone, if any
    Transform m_GoalBone;

    /// the blending weight for this limb
    float m_Weight;

    // -- lifecycle --
    void Awake() {
        // set deps
        c = GetComponentInParent<CharacterContainer>();
    }

    void Update() {
        if (!IsValid) {
            return;
        }

        m_StrideSystem.Update(Time.deltaTime);

        // blend the weight
        var isBlendingIn = m_StrideSystem.IsIdle;
        m_Weight = Mathf.MoveTowards(
            m_Weight,
            isBlendingIn ? 1.0f : 0.0f,
            Time.deltaTime / (isBlendingIn ? m_BlendInDuration : m_BlendOutDuration)
        );

        // AAA: blend ik
        m_Weight = isBlendingIn ? 1.0f : 0.0f;

        if (m_Goal <= AvatarIKGoal.RightFoot) {
            DebugDraw("limb", width: 2f);
        }
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

        // init system
        // TODO: unclear if we really want to init as our own anchor
        m_StrideSystem.Init(c, m_Goal, m_GoalBone.position, anchor: this);

        // error on misconfiguration
        if (!IsValid) {
            Log.Model.E($"{c.Name} - <limb: {m_Goal}> no matching bone");
        }
    }

    /// starts a new stride for the limb
    public void Move(CharacterBone anchor) {
        m_StrideSystem.Move(anchor);
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

        if (m_Weight != 0.0f) {
            m_Animator.SetIKPosition(
                m_Goal,
                m_StrideSystem.GoalPos
            );

            m_Animator.SetIKRotation(
                m_Goal,
                m_StrideSystem.GoalRot
            );
        }
    }

    // -- queries --
    /// if this limb has the dependencies it needs to apply ik
    public bool IsValid {
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

    /// the square length of the bone
    public float SqrLength {
        get => Vector3.SqrMagnitude(RootPos - GoalPos);
    }

    /// .
    public bool IsHeld {
        get => m_StrideSystem.IsHeld;
    }

    // -- debug --
    /// draw the debug line of this bone
    internal void DebugDraw(string name, float alpha = 1f, float width = 1f, int count = 1) {
        ThirdPerson.DebugDraw.PushLine(
            m_Goal.DebugName($"{name}-bone"),
            m_StrideSystem.GoalPos,
            transform.position,
            new DebugDraw.Config(m_Goal.DebugColor(alpha), tags: ThirdPerson.DebugDraw.Tag.Movement, width: width, count: count)
        );

        ThirdPerson.DebugDraw.Push(
            m_Goal.DebugName($"{name}-foot"),
            m_StrideSystem.GoalPos,
            new DebugDraw.Config(m_Goal.DebugColor(alpha), tags: ThirdPerson.DebugDraw.Tag.Movement, width: width * 2f, count: count)
        );
    }
}

static class CharacterLimbDebug {
    /// the debug name for a drawing
    internal static string DebugName(this AvatarIKGoal goal, string name) {
        var suffix = goal switch {
            AvatarIKGoal.LeftFoot => "fl",
            AvatarIKGoal.RightFoot => "fr",
            AvatarIKGoal.LeftHand => "hl",
            AvatarIKGoal.RightHand => "hr",
            _ => throw new ArgumentOutOfRangeException()
        };

        return $"{name}-{suffix}";
    }

    /// the debug color for a limb with given alpha (red is right)
    internal static Color DebugColor(this AvatarIKGoal goal, float alpha = 1f) {
        var color = goal switch {
            AvatarIKGoal.LeftFoot => Color.blue,
            AvatarIKGoal.RightFoot => Color.red,
            AvatarIKGoal.LeftHand => Color.cyan,
            AvatarIKGoal.RightHand => Color.magenta,
            _ => throw new ArgumentOutOfRangeException()
        };

        color.a = alpha;

        return color;
    }
}

}