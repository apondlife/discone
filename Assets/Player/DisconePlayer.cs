using UnityEngine;
using UnityAtoms.BaseAtoms;
using ThirdPerson;

/// the discone player
sealed class DisconePlayer: MonoBehaviour {
    // -- references --
    [Header("events")]
    [Tooltip("if the dialogue is active")]
    [SerializeField] BoolEvent m_IsDialogueActiveChanged;

    // -- references --
    [Header("references")]
    [Tooltip("the input source")]
    [SerializeField] PlayerInputSource m_InputSource;

    // -- lifecycle --
    void Awake() {
        m_IsDialogueActiveChanged.Register(OnIsDialogueActiveChanged);
    }

    // -- events --
    /// when the dialog becomes in/active
    void OnIsDialogueActiveChanged(bool isDialogueActive) {
        m_InputSource.enabled = !isDialogueActive;
    }
}