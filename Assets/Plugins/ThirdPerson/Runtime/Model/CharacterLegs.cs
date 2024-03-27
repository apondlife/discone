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

    CharacterLimb m_Curr;
    CharacterLimb m_Next;

    // -- lifecycle --
    void OnValidate() {
        m_Left.transform.localPosition = Vector3.zero;
        m_Right.transform.localPosition = Vector3.zero;

        m_Curr = m_Left;
        m_Next = m_Right;
    }

    void Update() {
        if (!m_Left.IsHeld || !m_Right.IsHeld) {
            return;
        }

        var rootPos = transform.position;

        m_Curr = m_Left;
        m_Next = m_Right;

        var currDist = Vector3.SqrMagnitude(m_Curr.Position - rootPos);
        var heldDist = Vector3.SqrMagnitude(m_Next.Position - rootPos);
        if (heldDist < currDist) {
            (m_Curr, m_Next) = (m_Next, m_Curr);
        }

        m_Next.Move(m_Curr.Position);
        m_Curr.Hold();

        (m_Curr, m_Next) = (m_Next, m_Curr);
    }
}

}