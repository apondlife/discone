using System;
using Soil;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {

/// center of mass? move character down?
/// an ik limb for the character model
public class CharacterLimb: MonoBehaviour, CharacterPart, CharacterBone {
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
    /// the containing character
    CharacterContainer c;

    /// the animator for this limb
    Animator m_Animator;

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
        var isBlendingIn = !m_StrideSystem.IsFree;
        m_Weight = Mathf.MoveTowards(
            m_Weight,
            isBlendingIn ? 1.0f : 0.0f,
            Time.deltaTime / (isBlendingIn ? m_BlendInDuration : m_BlendOutDuration)
        );

        // AAA: blend ik
        m_Weight = isBlendingIn ? 1.0f : 0.0f;

        if (m_Goal <= AvatarIKGoal.RightFoot) {
            Debug_Draw("limb", width: 2f);
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

    /// released the limb's hold if not already
    public void Release() {
        m_StrideSystem.Release();
    }

    /// set the current offset to translate the legs
    public void SetOffset(Vector3 offset) {
        m_StrideSystem.SetOffset(offset);
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
    public bool IsFree {
        get => m_StrideSystem.IsFree;
    }

    /// .
    public bool IsHeld {
        get => m_StrideSystem.IsHeld;
    }

    // -- debug --
    /// draw the debug line of this bone
    internal void Debug_Draw(string name, float alpha = 1f, float width = 1f, int count = 1) {
        DebugDraw.PushLine(
            m_Goal.Debug_Name($"{name}-bone"),
            m_StrideSystem.GoalPos,
            transform.position,
            new DebugDraw.Config(m_Goal.Debug_Color(alpha), tags: DebugDraw.Tag.Movement, width: width, count: count)
        );

        DebugDraw.Push(
            m_Goal.Debug_Name($"{name}-foot-{Debug_PhaseName()}"),
            m_StrideSystem.GoalPos,
            new DebugDraw.Config(Debug_PhaseColor(alpha), tags: DebugDraw.Tag.Movement, width: width * 3f, count: count)
        );
    }
    /// the debug color for a limb with given alpha (red is right)
    string Debug_PhaseName() {
        var name = "moving";
        if (m_StrideSystem.IsHeld) {
            name = "holding";
        } else if (m_StrideSystem.IsFree) {
            name = "free";
        }

        return name;
    }

    /// the debug color for a limb with given alpha (red is right)
    Color Debug_PhaseColor(float alpha = 1f) {
        var color = Color.green;
        if (m_StrideSystem.IsHeld) {
            color = Color.black;
        } else if (m_StrideSystem.IsFree) {
            color = Color.white;
        }

        color.a = alpha;

        return color;
    }
}

static class CharacterLimb_Debug {
    /// the debug name for a drawing
    internal static string Debug_Name(this AvatarIKGoal goal, string name) {
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
    internal static Color Debug_Color(this AvatarIKGoal goal, float alpha = 1f) {
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