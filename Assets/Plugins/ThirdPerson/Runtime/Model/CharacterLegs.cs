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

    [Tooltip("a minimum move speed applied every frame")]
    [SerializeField] float m_MinMoveSpeed;

    // -- props --
    /// the character's dependency container
    CharacterContainer c;

    // -- lifecycle --
    void Awake() {
        // set deps
        c = GetComponentInParent<CharacterContainer>();
    }

    void Update() {
        var offset = Vector3.zero;

        // set offset in move direction
        var v = c.State.Curr.SurfaceVelocity;
        if (c.Inputs.IsMoveActive && v.sqrMagnitude < m_MinMoveSpeed * m_MinMoveSpeed) {
            var moveDir = v != Vector3.zero ? v.normalized : c.State.Curr.Forward;
            offset = m_MinMoveSpeed * Time.deltaTime * moveDir;
        }

        m_Left.SetOffset(offset);
        m_Right.SetOffset(offset);

        // if both legs are held, start moving one
        if (m_Left.IsHeld && m_Right.IsHeld) {
            Switch();
        }
        // if the held leg becomes free, release the moving leg
        else if (m_Left.IsFree != m_Right.IsFree) {
            Release();
        }
    }

    // -- commands --
    /// switch the moving leg
    void Switch() {
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
            new DebugDraw.Config(color: Color.yellow, DebugDraw.Tag.Model, count: 100)
        );

        m_Left.Debug_Draw("legs", count: 100);
        m_Right.Debug_Draw("legs", count: 100);
    }

    /// make sure both legs are free
    void Release() {
        m_Left.Release();
        m_Right.Release();
    }
}

}