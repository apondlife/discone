using UnityEngine;

namespace ThirdPerson {

/// debug extensions for the character
public partial class Character {
    // -- debug - drawing --
    void Debug_Draw() {
        if (m_IsPaused) {
            return;
        }

        var pos = m_State.Position;
        DebugDraw.Push(s_Position, pos - 1f * Vector3.up, Vector3.up * 2f);
        DebugDraw.Push(s_Velocity, pos, m_State.Velocity);
        DebugDraw.Push(s_Force, pos, m_State.Force - m_Tuning.Gravity * Vector3.up);
        DebugDraw.Push(s_Inertia, pos, m_State.Inertia);
    }

    // -- config --
    // TODO: consider whether this should be applied via an attribute on the
    // state prop, via a scriptable object, &c
    static readonly DebugDraw.Value s_Position = new DebugDraw.Value(
        name: "position",
        color: Color.yellow,
        count: 100,
        minAlpha: 0.1f
    );

    static readonly DebugDraw.Value s_Velocity = new DebugDraw.Value(
        name: "velocity",
        color: Color.magenta,
        count: 50,
        lengthScale: 0.5f,
        minAlpha: 0.1f
    );

    static readonly DebugDraw.Value s_Force = new DebugDraw.Value(
        name: "force",
        color: new Color(1f, 0f, 0f, 0.5f),
        count: 50,
        lengthScale: 0.05f,
        minAlpha: 0.1f
    );

    static readonly DebugDraw.Value s_Inertia = new DebugDraw.Value(
        name: "inertia",
        count: 100,
        color: Color.green
    );
}

}