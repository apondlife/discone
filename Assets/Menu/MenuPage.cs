using UnityEngine;

namespace Discone.Ui {

/// a menu page
[RequireComponent(typeof(CanvasGroup))]
sealed class MenuPage: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the canvas group")]
    [SerializeField] CanvasGroup m_Group;

    [Tooltip("the elements on this page (set at runtime)")]
    [SerializeField] MenuElement[] m_Elements;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Elements = GetComponentsInChildren<MenuElement>();
    }

    void Start() {
        // the page is always visible, even if its contents are not (undo any
        // editor state)
        m_Group.alpha = 1.0f;
    }

    // -- commands --
    /// show or hide the page
    public void Show(float pct) {
        // don't block raycats when hidden
        m_Group.blocksRaycasts = pct != 0.0f;

        // update every element
        foreach (var element in m_Elements) {
            element.Show(pct);
        }
    }
}

}