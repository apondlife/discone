using UnityEngine;

namespace Discone.Ui {

/// a menu element on a page
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
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

}