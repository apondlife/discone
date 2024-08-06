using System;
using UnityEngine;

namespace ThirdPerson {

public partial class Limb {
    // -- lifecycle --
    void Debug_Update() {
        Debug_Draw("limb", width: 1f);
    }

    void Debug_UpdateIk() {
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = m_Goal.Debug_Color();
        Gizmos.DrawRay(RootPos, SearchDir * Mathf.Max(InitialLen, 1f));
    }

    // -- debug --
    /// draw the debug line of this bone
    internal void Debug_Draw(string name, float alpha = 1f, float width = 1f, int count = 1) {
        var goalColor = m_Goal.Debug_Color(alpha);

        var goalPos = GoalPos;
        var rootPos = RootPos;

        m_Goal.Debug_Tag()
            .Push(m_Goal.Debug_Name($"{name}-bone"), color: goalColor, width: width, count: count)
            .Line(rootPos, goalPos);

        var goalDir = Vector3.Normalize(goalPos - rootPos);
        var endPos = rootPos + goalDir * InitialLen;

        m_Goal.Debug_Tag()
            .Push(m_Goal.Debug_Name($"{name}-bone-end-{Debug_PhaseName()}"), color: Debug_PhaseColor(alpha), width: width + 3f, count: count)
            .Point(endPos);

        m_Goal.Debug_Tag()
            .Push(m_Goal.Debug_Name($"{name}-held-dist"), color: goalColor, width: width - 0.5f, count: count)
            .Ray(endPos, SearchDir * m_State.HeldDistance);
    }

    /// the debug color for a limb with given alpha (red is right)
    internal string Debug_PhaseName() {
        return m_System.Debug_PhaseName.ToLower();
    }

    /// the debug color for a limb with given alpha (red is right)
    internal Color Debug_PhaseColor(float alpha = 1f) {
        var color = Color.green;
        if (m_State.IsHeld) {
            color = Color.black;
        }
        else if (m_State.IsFree) {
            color = Color.white;
        }
        else if (!m_State.IsActive) {
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
            AvatarIKGoal.LeftFoot => DebugDraw.Walk,
            AvatarIKGoal.RightFoot => DebugDraw.Walk,
            AvatarIKGoal.LeftHand => DebugDraw.None,
            AvatarIKGoal.RightHand => DebugDraw.None,
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