using UnityEngine;

namespace ThirdPerson {

/// debug extensions for the character
public partial class Character {
    // -- debug - drawing --
    void Debug_Draw() {
        if (m_IsPaused) {
            return;
        }

        var pos = m_State.Curr.Position;
        DebugDraw.Push(s_Position, pos, Vector3.down);
        DebugDraw.Push(s_Velocity, pos, m_State.Curr.Velocity);
        DebugDraw.Push(s_Force, pos, m_State.Curr.Force - m_Tuning.Gravity * Vector3.up);
        DebugDraw.Push(s_Inertia, pos, m_State.Curr.Inertia);
    }

    // -- config --
    // TODO: consider whether this should be applied via an attribute on the
    // state prop, via a scriptable object, &c
    static readonly DebugDraw.Value s_Position = new(
        name: "~position",
        new DebugDraw.Config(
            color: Color.yellow,
            count: 100,
            width: 3f,
            scale: 0.01f,
            minAlpha: 0.1f
        )
    );

    static readonly DebugDraw.Value s_Velocity = new(
        name: "~velocity",
        new DebugDraw.Config(
            color: Color.magenta,
            count: 50,
            scale: 0.1f,
            minAlpha: 0.1f
        )
    );

    static readonly DebugDraw.Value s_Force = new(
        name: "~force",
        new DebugDraw.Config(
            color: new Color(1f, 0f, 0f, 0.5f),
            count: 50,
            scale: 0.05f,
            minAlpha: 0.1f
        )
    );

    static readonly DebugDraw.Value s_Inertia = new(
        name: "~inertia",
        new DebugDraw.Config(
            count: 100,
            color: Color.green,
            minAlpha: 0.1f
        )
    );
}

}