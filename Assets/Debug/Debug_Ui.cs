using ThirdPerson;
using UnityEngine;
using TMPro;

namespace Discone {

/// the debug ui
sealed class Debug_Ui: MonoBehaviour {
    // -- elements --
    [Header("elements")]
    [Tooltip("the tag label ui")]
    [SerializeField] TMP_Text m_TagLabel;

    // -- refs --
    [Header("refs")]
    [Tooltip("the debug drawings")]
    [SerializeField] DebugDraw m_DebugDraw;

    // -- lifecycle --
    void Update() {
        // draw the tags
        m_TagLabel.gameObject.SetActive(m_DebugDraw.IsEnabled);
        m_TagLabel.text = $"drawing: {m_DebugDraw.Tags.ToString().ToLower()}";
    }
}

}