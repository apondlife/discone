using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Discone.Ui {

/// the in-game menu
public class Menu: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the transition timer")]
    [SerializeField] AnimationTimer m_Transition;

    // -- refs --
    [Header("refs")]
    [Tooltip("the list of pages (set at runtime)")]
    [SerializeField] MenuPage[] m_Pages;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("when an offset page button is pressed")]
    [SerializeField] IntEvent m_OffsetPagePressed;

    // -- props --
    /// the current page index
    int m_CurrPage = 0;

    /// the previous page index
    int m_PrevPage = 0;

    /// the subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- lifecycle --
    void Awake() {
        // find pages
        m_Pages = GetComponentsInChildren<MenuPage>();
    }

    void Start() {
        // hide all but current page
        for (var i = 0; i < m_Pages.Length; i++) {
            var page = m_Pages[i];

            var enter = i == m_CurrPage;
            if (enter) {
                page.OnBeforeEnter();
            }

            page.Show(1.0f, enter: enter);
        }

        // bind events
        m_Subscriptions
            .Add(m_OffsetPagePressed, OnOffsetPagePressed);
    }

    void Update() {
        // run page transition
        var t = m_Transition;
        if (t.IsActive) {
            t.Tick();

            PrevPage.Show(t.Pct, enter: false);
            CurrPage.Show(t.Pct, enter: true);
        }
    }

    void OnDestroy() {
        // release events
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// transition to the page at the index
    void ChangeTo(int page) {
        // clamp page to range
        page = Mathf.Clamp(page, 0, m_Pages.Length);

        // update the page index
        m_PrevPage = m_CurrPage;
        m_CurrPage = page;

        // send events
        CurrPage.OnBeforeEnter();
        PrevPage.OnBeforeTransition();
        CurrPage.OnBeforeTransition();

        // init the transition
        m_Transition.Start();
    }

    // -- queries --
    /// the current page
    MenuPage CurrPage {
        get => m_Pages[m_CurrPage];
    }

    /// the previous page
    MenuPage PrevPage {
        get => m_Pages[m_PrevPage];
    }

    // -- events --
    /// when an offset page button is pressed
    void OnOffsetPagePressed(int offset) {
        ChangeTo(m_CurrPage + offset);
    }
}

}