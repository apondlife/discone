using Soil;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using ThirdPerson;

namespace Discone.Ui {

/// the in-game menu
[RequireComponent(typeof(MenuInput))]
public sealed class Menu: UIBehaviour {
    // -- constants --
    /// the index when there is no page
    const int k_PageNone = -1;

    // -- state --
    [Header("state")]
    [Tooltip("atom if the menu is open")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_IsMenuOpen")]
    [SerializeField] BoolVariable m_IsOpen;

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the transition timer")]
    [SerializeField] EaseTimer m_Transition;

    [Tooltip("if the menu should be open on start")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_StartOn")]
    [SerializeField] bool m_IsOpenOnStart;

    // -- refs --
    [Header("refs")]
    [Tooltip("the main menu")]
    [SerializeField] GameObject m_Main;

    [Tooltip("the overlay")]
    [SerializeField] CanvasGroup m_Overlay;

    [Tooltip("the list of pages (set at runtime)")]
    [SerializeField] Page[] m_Pages;

    // -- dispatched --
    [Header("dispatched")]
    [Tooltip("an event to initiate a server connection")]
    [SerializeField] VoidEvent m_ConnectToServer;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("when an offset page button is pressed")]
    [SerializeField] IntEvent m_OffsetPagePressed;

    [Tooltip("when an menu action is dispatched")]
    [SerializeField] MenuActionEvent m_Action;

    // -- props --
    /// the current page index
    int m_CurrPage = k_PageNone;

    /// the previous page index
    int m_PrevPage = k_PageNone;

    /// the saved page when hidden
    int m_SavedPage = 0;

    /// the dialog page
    DialogPage m_DialogPage;

    /// the menu input
    MenuInput m_Input;

    /// the subscriptions
    DisposeBag m_Subscriptions = new DisposeBag();

    // -- lifecycle --
    protected override void Awake() {
        base.Awake();

        // enable the inputs
        m_Input = GetComponent<MenuInput>();

        // find pages
        m_Pages = m_Main.GetComponentsInChildren<Page>(includeInactive: true);

        // find the dialog page
        var dialogPage = m_Pages[m_Pages.Length - 1].GetComponent<DialogPage>();
        if (dialogPage == null) {
            Log.Menu.E($"the dialog page was not the last page");
        }

        m_DialogPage = dialogPage;
    }

    protected override void Start() {
        base.Start();

        // set initial visibility of all pages
        for (var i = 0; i < m_Pages.Length; i++) {
            var page = m_Pages[i];
            var enter = IsOpen && i == m_CurrPage;
            page.Show(1.0f, enter);
            page.OnAfterTransition(enter);
        }

        // bind events
        m_Subscriptions
            .Add(m_Input.Toggle, OnTogglePressed)
            .Add(m_Input.Connect, OnConnectPressed)
            .Add(m_IsOpen.Changed, OnIsOpenChanged)
            .Add(m_Action, OnAction)
            .Add(m_OffsetPagePressed, OnOffsetPagePressed);

        // set initial state
        if (m_IsOpenOnStart) {
            Toggle(true);
        }
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
                    IsOpen = false;
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
        Toggle(!IsOpen);
    }

    /// toggle the menu, optionally forcing a state
    void Toggle(bool isVisible) {
        // don't do this if in a transition
        if (m_Transition.IsActive) {
            return;
        }

        // ignore redundant updates
        if (IsOpen == isVisible) {
            return;
        }

        // hide the menu
        if (!isVisible) {
            m_SavedPage = m_CurrPage;
            ChangeTo(k_PageNone);
        }
        // show the menu
        else {
            IsOpen = true;
            ChangeTo(m_SavedPage);
        }
    }

    /// start a transition to the page at the index
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

    /// change to the dialog page w/ the dialog
    void ShowDialog(MenuDialog dialog) {
        if (m_DialogPage == null) {
            return;
        }

        // show the dialog, restoring current page on complete
        var curr = m_CurrPage;
        m_DialogPage.Show(dialog, () => {
            ChangeTo(curr);
        });

        /// switch to the dialog page
        ChangeTo(m_Pages.Length - 1);
    }

    // -- queries --
    /// the current page component, if any
    Page CurrPage {
        get => PageAt(m_CurrPage);
    }

    /// the previous page component, if any
    Page PrevPage {
        get => PageAt(m_PrevPage);
    }

    /// get the page at the index, if any
    Page PageAt(int index) {
        return index != k_PageNone ? m_Pages[index] : null;
    }

    /// if the menu is transitioning from closed to open
    bool IsShowing {
        get => m_CurrPage != -1 && m_PrevPage == -1;
    }

    /// if the menu is transitioning from open to closed
    bool IsHiding {
        get => m_CurrPage == -1 && m_PrevPage != -1;
    }

    // -- props/hot --
    /// .
    bool IsOpen {
        get => m_IsOpen.Value;
        set => m_IsOpen.Value = value;
    }

    // -- events --
    /// when the toggle input is pressed
    void OnTogglePressed(InputAction.CallbackContext _) {
        Toggle();
    }

    /// when the connnect input is pressed
    void OnConnectPressed(InputAction.CallbackContext _) {
        if (IsOpen) {
            m_ConnectToServer.Raise();
        }
    }

    /// when a menu action is dispatched
    void OnAction(MenuAction action) {
        switch (action) {
        case MenuAction.ShowDialog a:
            ShowDialog(a.Dialog); break;
        default:
            break;
        }
    }

    /// when the is open state changes
    void OnIsOpenChanged(bool isOpen) {
        m_Main.SetActive(isOpen);
    }

    /// when an offset page button is pressed
    void OnOffsetPagePressed(int offset) {
        ChangeTo(m_CurrPage + offset);
    }
}

}