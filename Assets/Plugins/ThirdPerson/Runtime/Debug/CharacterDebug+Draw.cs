using UnityEngine;

namespace ThirdPerson {

/// debug extensions for the character
public partial class Character<InputFrame> {
    // -- debug/drawing --
    void Debug_Draw() {
        if (m_IsPaused) {
            return;
        }

        var curr = m_State.Curr;
        var pos = curr.Position;

        DebugDraw.Default
            .Push("~position", color: Color.yellow, count: 100, width: 3f, scale: 0.01f, minAlpha: 0.1f)
            .Point(pos);

        DebugDraw.Default
            .Push("~velocity", color: Color.magenta, count: 50, scale: 0.1f, minAlpha: 0.1f)
            .Ray(pos, curr.Velocity);

        DebugDraw.Default
            .Push("~force", color: new Color(1f, 0f, 0f, 0.5f), count: 50, scale: 0.05f, minAlpha: 0.1f)
            .Ray(pos, curr.Force.Continuous - m_Tuning.Gravity * Vector3.up);

        DebugDraw.Default
            .Push("~inertia", color: Color.green, count: 100, minAlpha: 0.1f)
            .Ray(pos, curr.Inertia * -curr.MainSurface.Normal);
    }
}

}