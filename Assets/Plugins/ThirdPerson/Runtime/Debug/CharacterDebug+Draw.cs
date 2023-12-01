using UnityEngine;

namespace ThirdPerson {

/// debug extensions for the character
public partial class Character {
    // -- constants --
    /// the debug draw key
    const KeyCode k_Debug_Draw = KeyCode.Alpha0;

    // -- debug - drawing --
    [Header("debug - drawing")]
    [Tooltip("if this character is drawing")]
    [SerializeField] bool m_IsDrawing;

    void Debug_DrawInput() {
        // toggle drawing on debug press
        if (UnityEngine.Input.GetKeyDown(k_Debug_Draw)) {
            m_IsDrawing = !m_IsDrawing;
        }
    }

    void Debug_Draw() {
        if (!m_IsDrawing || m_IsPaused) {
            return;
        }

        var pos = m_State.Position;
        DebugDraw.Push(s_Position, pos - 1f * Vector3.up, Vector3.up * 2f);
        DebugDraw.Push(s_Velocity, pos, m_State.Velocity);
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
        minAlpha: 0.1f
    );

    static readonly DebugDraw.Value s_Inertia = new DebugDraw.Value(
        name: "inertia",
        count: 100,
        color: Color.green
    );
}

}