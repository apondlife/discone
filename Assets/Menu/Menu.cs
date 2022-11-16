using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Discone.Ui {

/// the in-game menu
[RequireComponent(typeof(MenuInput))]
sealed class Menu: UIBehaviour {
    // -- constants --
    /// the index when there is no page
    const int k_PageNone = -1;

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the transition timer")]
    [SerializeField] AnimationTimer m_Transition;

    // -- refs --
    [Header("refs")]
    [Tooltip("the main menu")]
    [SerializeField] GameObject m_Main;

    [Tooltip("the overlay")]
    [SerializeField] CanvasGroup m_Overlay;

    [Tooltip("the list of pages (set at runtime)")]
    [SerializeField] Page[] m_Pages;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("when an offset page button is pressed")]
    [SerializeField] IntEvent m_OffsetPagePressed;

    // -- props --
    /// the current page index
    int m_CurrPage = k_PageNone;

    /// the previous page index
    int m_PrevPage = k_PageNone;

    /// the saved page when hidden
    int m_SavedPage = 0;

    /// the menu input
    MenuInput m_Input;

    /// the subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- lifecycle --
    protected override void Awake() {
        base.Awake();

        // enable the inputs
        m_Input = GetComponent<MenuInput>();

        // find pages
        m_Pages = m_Main.GetComponentsInChildren<Page>(includeInactive: true);
    }

    protected override void Start() {
        base.Start();

        // set initial visibility of all pages
        for (var i = 0; i < m_Pages.Length; i++) {
            var page = m_Pages[i];
            var enter = IsVisible && i == m_CurrPage;
            page.Show(1.0f, enter);
            page.OnAfterTransition(enter);
        }

        // bind events
        m_Subscriptions
            .Add(m_Input.Toggle, OnTogglePressed)
            .Add(m_OffsetPagePressed, OnOffsetPagePressed);
    }

    void Update() {
        // run page transition
        var t = m_Transition;
        if (t.IsActive) {
            t.Tick();

            // update the overlay
            var pct = t.Pct;
            if (IsShowing) {
                m_Overlay.alpha = pct;
            } else if (IsHiding) {
                m_Overlay.alpha = 1.0f - pct;
            }

            // update each page
            PrevPage?.Show(pct, enter: false);
            CurrPage?.Show(pct, enter: true);

            // if finished
            if (!t.IsActive) {
                // send events
                PrevPage?.OnAfterTransition(enter: false);
                CurrPage?.OnAfterTransition(enter: true);

                // if hidden, disable the menu
                if (IsHiding) {
                    IsVisible = false;
                }
            }
        }
    }

    protected override void OnDestroy() {
        base.OnDestroy();

        // release events
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// toggle the menu
    void Toggle() {
        // don't do this if in a transition
        if (m_Transition.IsActive) {
            return;
        }

        // hide the menu
        if (IsVisible) {
            m_SavedPage = m_CurrPage;
            ChangeTo(k_PageNone);
        }
        // show the menu
        else {
            IsVisible = true;
            ChangeTo(m_SavedPage);
        }
    }

    /// transition to the page at the index
    void ChangeTo(int page) {
        // don't do this if in a transition
        if (m_Transition.IsActive) {
            return;
        }

        // clamp page to range
        if (page != k_PageNone) {
            page = Mathf.Clamp(page, 0, m_Pages.Length);
        }

        // update the page index
        m_PrevPage = m_CurrPage;
        m_CurrPage = page;

        // send events
        PrevPage?.OnBeforeTransition(enter: false);
        CurrPage?.OnBeforeTransition(enter: true);

        // init the transition
        m_Transition.Start();
    }

    // -- queries --
    /// the current page, if any
    Page CurrPage {
        get => PageAt(m_CurrPage);
    }

    /// the previous page, if any
    Page PrevPage {
        get => PageAt(m_PrevPage);
    }

    /// get the page at the index, if any
    Page PageAt(int index) {
        return index != k_PageNone ? m_Pages[index] : null;
    }

    /// if the menu is showing
    bool IsShowing {
        get => m_CurrPage != -1 && m_PrevPage == -1;
    }

    /// if the menu is hiding
    bool IsHiding {
        get => m_CurrPage == -1 && m_PrevPage != -1;
    }

    // -- props/hot --
    /// if the menu is visible
    bool IsVisible {
        get => m_Main.activeSelf;
        set => m_Main.SetActive(value);
    }

    // -- events --
    /// when the toggle button is pressed
    void OnTogglePressed(InputAction.CallbackContext _) {
        Toggle();
    }

    /// when an offset page button is pressed
    void OnOffsetPagePressed(int offset) {
        ChangeTo(m_CurrPage + offset);
    }
}

}