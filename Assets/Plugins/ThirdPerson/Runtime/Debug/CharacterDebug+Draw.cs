using UnityEngine;

namespace ThirdPerson {

/// debug extensions for the character
public partial class Character {
    // -- debug - drawing --
    [Header("debug - drawing")]
    [Tooltip("if this character is drawing")]
    [SerializeField] bool m_IsDrawing;

    void Debug_Draw() {
        if (!m_IsDrawing) {
            return;
        }

        var pos = m_State.Position;
        DebugDraw.Push("position", pos - 1f * Vector3.up, Vector3.up * 2f);
        DebugDraw.Push("velocity", pos, m_State.Velocity);
        DebugDraw.Push("acceleration", pos, m_State.Acceleration);
        DebugDraw.Push("inertia", pos, m_State.Inertia);
    }
}

}