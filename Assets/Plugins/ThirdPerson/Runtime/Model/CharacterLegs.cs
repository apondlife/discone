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
        if (!m_Left.IsHeld || !m_Right.IsHeld) {
            return;
        }

        var rootPos = transform.position;

        var curr = m_Left;
        var held = m_Right;

        var currDist = Vector3.SqrMagnitude(curr.GoalPos - rootPos);
        var heldDist = Vector3.SqrMagnitude(held.GoalPos - rootPos);
        if (heldDist < currDist) {
            (curr, held) = (held, curr);
        }

        held.Move(curr);
        curr.Hold();
    }
}

}