using UnityEngine;
using UnityEngine.EventSystems;

namespace Discone.Ui {

/// a menu element on a page
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
sealed class Component: UIBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the distance range to jitter the position")]
    [SerializeField] ThirdPerson.RangeCurve m_JitterDist;

    [Tooltip("an axis to jitter the position along; 0 is right, 1 is up; outside (0..1) is no axis (a random direction)")]
    [SerializeField] float m_JitterDist_Axis;

    [Tooltip("the angle range to jitter the rotation in degrees")]
    [SerializeField] ThirdPerson.RangeCurve m_JitterRotation;

    [Tooltip("if the rotation jitter is in world-space")]
    [SerializeField] bool m_JitterRotation_IsLocal;

    [Tooltip("the transition distance range")]
    [SerializeField] ThirdPerson.RangeCurve m_TransitionDist;

    // -- props --
    /// the canvas group
    CanvasGroup m_Group;

    /// the content element
    RectTransform m_Content;

    /// the component's initial position
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

        // set intial state
        ChangeJitter();
        ChangeTranslation();
    }

    // -- commands --
    /// show or hide the component
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

    /// change the component jitter
    void ChangeJitter() {
        // jitter rotation
        var rot = m_JitterRotation.Evaluate(Random.value);
        if (m_JitterRotation_IsLocal) {
            m_Content.localEulerAngles = new Vector3(0f, 0f, rot);
        } else {
            m_Content.eulerAngles = new Vector3(0f, 0f, rot);
        }

        // jitter position
        var dir = Vector3.up;
        if (m_JitterDist_Axis < 0f || m_JitterDist_Axis > 1f) {
            dir = Random.insideUnitCircle;
        } else {
            dir = Mathf.Sign(Random.value - 0.5f) * Vector3.Normalize(
                m_Content.up * m_JitterDist_Axis +
                m_Content.right * (1f - m_JitterDist_Axis)
            );
        }

        var pos = dir * m_JitterDist.Evaluate(Random.value);
        m_Content.anchoredPosition = pos;

        // set initial pos
        m_InitialPos = m_Content.anchoredPosition;
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

    // -- events --
    /// when an element is about to enter the screen
    public void OnBeforeEnter() {
        ChangeJitter();
    }
}

}