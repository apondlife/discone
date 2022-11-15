using UnityEngine;
using UnityEngine.EventSystems;

namespace Discone.Ui {

/// a menu element on a page
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
sealed class MenuElement: UIBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the jitter distance range")]
    [SerializeField] ThirdPerson.RangeCurve m_JitterDist;

    [Tooltip("the jitter rotation range")]
    [SerializeField] ThirdPerson.RangeCurve m_JitterRotation;

    [UnityEngine.Serialization.FormerlySerializedAs("m_DistanceRange")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_TranslationAmount")]
    [Tooltip("the transition distance range")]
    [SerializeField] ThirdPerson.RangeCurve m_TransitionDist;

    // -- props --
    /// the canvas group
    CanvasGroup m_Group;

    /// the content element
    RectTransform m_Content;

    /// the element's initial position
    Vector3 m_InitialPos;

    /// the current transition
    Vector3 m_Translation;

    // -- lifecycle --
    protected override void Awake() {
        base.Awake();

        // set props
        m_Group = GetComponent<CanvasGroup>();
        m_Content = FindContent();
    }

    protected override void Start() {
        base.Start();

        // jitter rotation
        var rot = m_Content.localEulerAngles;
        rot.z = m_JitterRotation.Evaluate(Random.value);
        m_Content.localEulerAngles = rot;

        // jitter position
        var pos = m_Content.anchoredPosition;
        pos += Random.insideUnitCircle * m_JitterDist.Evaluate(Random.value);
        m_Content.anchoredPosition = pos;

        // set initial pos
        m_InitialPos = m_Content.anchoredPosition;

        // set intial state
        ChangeTranslation();
    }

    // -- commands --
    /// show or hide the element
    public void Show(float pct, bool enter) {
        // update alpha
        var alpha = enter ? pct : 1.0f - pct;
        m_Group.alpha = alpha;

        // update pos
        var t = m_Content.transform as RectTransform;
        var k = enter ? 1.0f - pct : pct;
        t.anchoredPosition = m_InitialPos + m_Translation * k;

        // once we reach an edge, pick a new translation (ptential bug if we
        // can hit the edge and still continue the transition, e.g. a reverse)
        if (pct == 0.0f || pct == 1.0f) {
            ChangeTranslation();
        }
    }

    /// pick a new transition ray
    void ChangeTranslation() {
        var dir = Random.insideUnitCircle;
        var len = m_TransitionDist.Evaluate(Random.value);
        m_Translation = dir * len;
    }

    // -- queries --
    /// find the lone content element
    RectTransform FindContent() {
        // find the child
        var t = transform;
        var n = t.childCount;
        if (n != 1) {
            Debug.LogError($"[menuuu] element `{name}' must have exactly one content element");
        }

        var content = t.GetChild(0) as RectTransform;
        if (content == null) {
            Debug.LogError($"[menuuu] element `{name}` must have a rect transform as content");
        }

        return content;
    }
}

}