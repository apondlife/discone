using UnityEngine;
using UnityEngine.EventSystems;

namespace Discone.Ui {

/// a menu page
[RequireComponent(typeof(CanvasGroup))]
sealed class MenuPage: UIBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the canvas group")]
    [SerializeField] CanvasGroup m_Group;

    [Tooltip("the elements on this page (set at runtime)")]
    [SerializeField] MenuElement[] m_Elements;

    [Tooltip("the page buttons on this page (set at runtime)")]
    [SerializeField] PageButton[] m_Buttons;

    // -- props --
    /// the page index
    int m_Index;

    // -- lifecycle --
    protected override void Awake() {
        base.Awake();

        // set props
        m_Index = transform.GetSiblingIndex();
        m_Group = GetComponent<CanvasGroup>();
        m_Elements = GetComponentsInChildren<MenuElement>();
        m_Buttons = GetComponentsInChildren<PageButton>();
    }

    protected override void Start() {
        base.Start();

        // the page is always visible, even if its contents are not (undo any
        // editor state)
        m_Group.alpha = 1.0f;
    }

    // -- commands --
    /// show or hide the page
    public void Show(float pct, bool enter) {
        // block raycasts when visible
        m_Group.blocksRaycasts = enter;

        // update every element
        foreach (var element in m_Elements) {
            element.Show(pct, enter);
        }
    }

    // -- events --
    /// when a page is about to enter the screen
    public void OnBeforeEnter() {
        foreach (var button in m_Buttons) {
            button.OnBeforeEnter();
        }
    }

    /// when a page is about to exit the screen
    public void OnBeforeExit() {
    }

    /// when a page is about to transition
    public void OnBeforeTransition() {
    }
}

}