using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.UI;

/// the in-game menu
[ExecuteAlways]
public class Menu: MonoBehaviour {
    // -- constants --
    // the sentinel for no transition
    const float k_TransitionNone = -1.0f;

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the page transition duration")]
    [SerializeField] float m_TransitionDuration;

    [Tooltip("the page transition curve")]
    [SerializeField] AnimationCurve m_TransitionCurve;

    // -- refs --
    [Header("refs")]
    [Tooltip("the list of pages (set at runtime)")]
    [SerializeField] MenuPage[] m_Pages;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("set the current page")]
    [SerializeField] IntEvent m_DebugPage;

    [Tooltip("when an offset page button is pressed")]
    [SerializeField] IntEvent m_OffsetPagePressed;

    // -- props --
    /// the current page index
    int m_CurrPage = 0;

    /// the previous page index
    int m_PrevPage = 0;

    /// the time the page transition began
    float m_TransitionStartTime = k_TransitionNone;

    /// the subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- lifecycle --
    void Awake() {
        // find pages
        m_Pages = GetComponentsInChildren<MenuPage>();

        // hide all but current page
        for (var i = 0; i < m_Pages.Length; i++) {
            m_Pages[i].Show(i == m_CurrPage ? 1.0f : 0.0f);
        }
    }

    void Start() {
        // bind events
        m_Subscriptions
            .Add(m_DebugPage, OnDebugPage)
            .Add(m_OffsetPagePressed, OnOffsetPagePressed);
    }

    void Update() {
        // run page transition
        if (m_TransitionStartTime != k_TransitionNone) {
            // calculate interpolant based on duration
            var k = (Time.time - m_TransitionStartTime) / m_TransitionDuration;

            // if finished, end transition
            if (k >= 1.0f) {
                k = 1.0f;
                m_TransitionStartTime = k_TransitionNone;
            }

            // update pages
            m_Pages[m_PrevPage].Show(m_TransitionCurve.Evaluate(1.0f - k));
            m_Pages[m_CurrPage].Show(m_TransitionCurve.Evaluate(k));
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
        var max = m_Pages.Length;
        page = Mathf.Clamp(page, 0, max);

        // update the page
        m_PrevPage = m_CurrPage;
        m_CurrPage = page;

        // init the transition
        m_TransitionStartTime = Time.time;
    }


    [ContextMenu("Reset")]
    void Reset() {
        Awake();
    }

    // -- events --
    /// when an offset page button is pressed
    void OnOffsetPagePressed(int offset) {
        ChangeTo(m_CurrPage + offset);
    }

    /// when the debug page event fires
    void OnDebugPage(int page) {
        ChangeTo(page);
    }
}
