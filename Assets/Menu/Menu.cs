using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.UI;

public class Menu: MonoBehaviour {
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

    /// the current scroll offset
    float m_ScrollOffset;

    /// the subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- lifecycle --
    void Start() {
        // bind events
        m_Subscriptions
            .Add(m_DebugPage, OnDebugPage)
            .Add(m_OffsetPagePressed, OnOffsetPagePressed);
    }

    void OnDestroy() {
        // release events
        m_Subscriptions.Dispose();
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
        m_ScrollOffset = (float)page / (max - 1);

        // unity's scroll rect goes from 1 to 0, so we invert it here
        m_Scroll.verticalNormalizedPosition = 1.0f - m_ScrollOffset;
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
