using System;
using UnityEngine;

namespace ThirdPerson {

public partial class CharacterLimb {
    // -- lifecycle --
    void Debug_Update() {
        if (m_Goal <= AvatarIKGoal.RightFoot) {
            Debug_Draw("limb", width: 2f);
        }
    }

    // -- debug --
    /// draw the debug line of this bone
    internal void Debug_Draw(string name, float alpha = 1f, float width = 1f, int count = 1) {
        DebugDraw.PushLine(
            m_Goal.Debug_Name($"{name}-bone"),
            m_StrideSystem.GoalPos,
            transform.position,
            new DebugDraw.Config(m_Goal.Debug_Color(alpha), tags: DebugDraw.Tag.Model, width: width, count: count)
        );

        DebugDraw.Push(
            m_Goal.Debug_Name($"{name}-foot-{Debug_PhaseName()}"),
            m_StrideSystem.GoalPos,
            new DebugDraw.Config(Debug_PhaseColor(alpha), tags: DebugDraw.Tag.Model, width: width * 3f, count: count)
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