using Soil;
using UnityEngine;

namespace ThirdPerson {

/// debug extensions for the character
public partial class Character {
    // -- debug/drawing --
    void Debug_Draw() {
        if (m_IsPaused) {
            return;
        }

        var pos = m_State.Curr.Position;
        DebugDraw.Push("~position", pos, s_Position);
        DebugDraw.Push("~velocity", pos, m_State.Curr.Velocity, s_Velocity);
        DebugDraw.Push("~force", pos, m_State.Curr.Force - m_Tuning.Gravity * Vector3.up, s_Force);
        DebugDraw.Push("~inertia", pos, m_State.Curr.Inertia * -m_State.Curr.MainSurface.Normal, s_Inertia);
    }

    // -- config --
    // TODO: consider whether this should be applied via an attribute on the
    // state prop, via a scriptable object, &c
    static readonly DebugDraw.Config s_Position = new(
        color: Color.yellow,
        tags: DebugDraw.Tag.Default,
        count: 100,
        width: 3f,
        scale: 0.01f,
        minAlpha: 0.1f
    );

    static readonly DebugDraw.Config s_Velocity = new(
        color: Color.magenta,
        tags: DebugDraw.Tag.Default,
        count: 50,
        scale: 0.1f,
        minAlpha: 0.1f
    );

    static readonly DebugDraw.Config s_Force = new(
        color: new Color(1f, 0f, 0f, 0.5f),
        tags: DebugDraw.Tag.Default,
        count: 50,
        scale: 0.05f,
        minAlpha: 0.1f
    );

    static readonly DebugDraw.Config s_Inertia = new(
        color: Color.green,
        tags: DebugDraw.Tag.Default,
        count: 100,
        minAlpha: 0.1f
    );
}

}