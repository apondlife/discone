using UnityEngine;

/// a menu element on a page
[ExecuteAlways]
[RequireComponent(typeof(CanvasGroup))]
sealed class MenuElement: MonoBehaviour {
    // -- props --
    /// the canvas group
    CanvasGroup m_Group;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Group = GetComponent<CanvasGroup>();
    }

    // -- commands --
    /// show or hide the element
    public void Show(float pct) {
        m_Group.alpha = pct;
    }
}