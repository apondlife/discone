using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.UI;

/// the in-game menu
[ExecuteAlways]
public class Menu: MonoBehaviour {
    // -- constants --
    // the sentinel for no transition
    const float k_TransitionNone = -1.0f;

    // -- state --
    [Header("state")]
    [Tooltip("the current scroll offset")]
    [SerializeField] float m_Offset;

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the page transition duration")]
    [SerializeField] float m_TransitionDuration;

    [Tooltip("the page transition curve")]
    [SerializeField] AnimationCurve m_TransitionCurve;

    // -- refs --
    [Header("refs")]
    [Tooltip("the scroll rect")]
    [SerializeField] ScrollRect m_Scroll;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("set the current page")]
    [SerializeField] IntEvent m_DebugPage;

    [Tooltip("when an offset page button is pressed")]
    [SerializeField] IntEvent m_OffsetPagePressed;

    // -- props --
    /// the current page index
    int m_Page;

    /// the time the page transition began
    float m_TransitionStartTime = k_TransitionNone;

    /// the page transition's start offset
    float m_TransitionOffset;

    /// the page transitions's offset delta
    float m_TransitionDelta;

    /// the subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- lifecycle --
    void Awake() {
        // init state
        m_Offset = 0.0f;
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

            // apply the curved transition
            m_Offset = m_TransitionOffset + m_TransitionDelta * m_TransitionCurve.Evaluate(k);
        }

        // update scroll position; unity's scroll offset domain is [1, 0]
        var dest = 1.0f - m_Offset;
        m_Scroll.verticalNormalizedPosition = dest;
    }

    void OnDestroy() {
        // release events
        m_Subscriptions.Dispose();
    }

    void OnValidate() {
        // validate state
        m_Offset = Mathf.Clamp01(m_Offset);
    }

    // -- commands --
    /// transition to the page at the index
    void ChangeTo(int page) {
        // clamp page to range
        var max = m_Scroll.content.childCount;
        page = Mathf.Clamp(page, 0, max);

        // update the page
        m_Page = page;

        // unity's scroll rect goes from the top of the first object to the
        // bottom of the last so for this number it seems like there's one fewer
        // page
        var dst = (float)page / (max - 1);

        // init the transition
        m_TransitionOffset = m_Offset;
        m_TransitionDelta = dst - m_Offset;
        m_TransitionStartTime = Time.time;
    }

    // -- events --
    /// when an offset page button is pressed
    void OnOffsetPagePressed(int offset) {
        ChangeTo(m_Page + offset);
    }

    /// when the debug page event fires
    void OnDebugPage(int page) {
        ChangeTo(page);
    }
}
