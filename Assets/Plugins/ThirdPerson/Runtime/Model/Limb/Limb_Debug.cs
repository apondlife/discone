using System;
using UnityEngine;

namespace ThirdPerson {

public partial class Limb {
    // -- lifecycle --
    void Debug_Update() {
        Debug_Draw("limb", width: 1f);
    }

    void Debug_ApplyIk() {
    }

    // -- debug --
    /// draw the debug line of this bone
    internal void Debug_Draw(string name, float alpha = 1f, float width = 1f, int count = 1) {
        var goalPos = m_StrideSystem.GoalPos;
        var rootPos = transform.position;
        var goalDir = Vector3.Normalize(goalPos - rootPos);

        DebugDraw.PushLine(
            m_Goal.Debug_Name($"{name}-bone"),
            rootPos,
            goalPos,
            new DebugDraw.Config(m_Goal.Debug_Color(alpha), tags: m_Goal.Debug_Tag(), width: width, count: count)
        );

        var endPos = rootPos + goalDir * InitialLen;

        DebugDraw.Push(
            m_Goal.Debug_Name($"{name}-bone-end-{Debug_PhaseName()}"),
            endPos,
            new DebugDraw.Config(Debug_PhaseColor(alpha), tags: m_Goal.Debug_Tag(), width: width + 3f, count: count)
        );

        DebugDraw.Push(
            m_Goal.Debug_Name($"{name}-held-dist"),
            endPos,
            SearchDir * m_StrideSystem.HeldDistance,
            new DebugDraw.Config(m_Goal.Debug_Color(alpha), tags: m_Goal.Debug_Tag(), width: width - 0.5f, count: count)
        );
    }

    /// the debug color for a limb with given alpha (red is right)
    internal string Debug_PhaseName() {
        return m_StrideSystem.Debug_PhaseName.ToLower();
    }

    /// the debug color for a limb with given alpha (red is right)
    internal Color Debug_PhaseColor(float alpha = 1f) {
        var color = Color.green;
        if (m_StrideSystem.IsHeld) {
            color = Color.black;
        } else if (m_StrideSystem.IsFree) {
            color = Color.white;
        } else if (!m_StrideSystem.IsActive) {
            color = Color.red;
        }

        color.a = alpha;

        return color;
    }
}

static class Limb_Debug {
    /// the debug name for a drawing
    internal static string Debug_Name(this AvatarIKGoal goal, string name) {
        var key = goal switch {
            AvatarIKGoal.LeftFoot => "fl",
            AvatarIKGoal.RightFoot => "fr",
            AvatarIKGoal.LeftHand => "hl",
            AvatarIKGoal.RightHand => "hr",
            _ => throw new ArgumentOutOfRangeException()
        };

        return $"limb-{key}-{name}";
    }

    /// the debug tag for a limb
    internal static DebugDraw.Tag Debug_Tag(this AvatarIKGoal goal) {
        return goal switch {
            AvatarIKGoal.LeftFoot => DebugDraw.Tag.Walk,
            AvatarIKGoal.RightFoot => DebugDraw.Tag.Walk,
            AvatarIKGoal.LeftHand => DebugDraw.Tag.None,
            AvatarIKGoal.RightHand => DebugDraw.Tag.None,
            _ => throw new ArgumentOutOfRangeException()
        };
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