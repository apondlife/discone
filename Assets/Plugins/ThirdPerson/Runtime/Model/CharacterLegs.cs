using UnityEngine;

namespace ThirdPerson {

/// a pair of legs working in unison
class CharacterLegs: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the left leg")]
    [SerializeField] CharacterLimb m_Left;

    [Tooltip("the right leg")]
    [SerializeField] CharacterLimb m_Right;

    // -- lifecycle --
    void Update() {
        if (!(m_Left.IsHeld && m_Right.IsHeld)) {
            return;
        }

        // move leg that is furthest away
        var (move, hold) = m_Left.SqrLength > m_Right.SqrLength
            ? (m_Left, m_Right)
            : (m_Right, m_Left);

        move.Move(hold);

        // draw hips & legs on move
        DebugDraw.PushLine(
            "legs-hips",
            m_Left.RootPos,
            m_Right.RootPos,
            new DebugDraw.Config(color: Color.yellow, DebugDraw.Tag.Movement, count: 100)
        );

        m_Left.DebugDraw("legs", count: 100);
        m_Right.DebugDraw("legs", count: 100);

    }
}

}