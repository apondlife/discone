using UnityEngine;

namespace Discone.Ui {

/// a menu element on a page
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
sealed class MenuElement: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the transition distance range")]
    [SerializeField] ThirdPerson.RangeCurve m_DistanceRange;

    // -- props --
    /// the canvas group
    CanvasGroup m_Group;

    /// the element's initial position
    Vector3 m_InitialPos;

    /// the current transition translation
    Vector3 m_Translation;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Group = GetComponent<CanvasGroup>();
        m_InitialPos = transform.position;

        if (name == "Audio" || name == "Credits") {
            Debug.Log($"name {name} pos {transform.position} local {transform.localPosition} anchored {(transform as RectTransform).anchoredPosition}");
        }
    }

    void Start() {
        ChangeTranslation();
    }

    // -- commands --
    /// show or hide the element
    public void Show(float pct, bool enter) {
        // update alpha
        var alpha = enter ? pct : 1.0f - pct;
        m_Group.alpha = alpha;

        // update pos
        var t = transform;
        var k = enter ? 1.0f - pct : pct;
        t.position = m_InitialPos + m_Translation * k;

        // once we reach an edge, pick a new translation (ptential bug if we
        // can hit the edge and still continue the transition, e.g. a reverse)
        if (pct == 0.0f || pct == 1.0f) {
            ChangeTranslation();
        }
    }

    /// pick a new transition ray
    void ChangeTranslation() {
        var dir = Random.insideUnitCircle;
        var len = m_DistanceRange.Evaluate(Random.value);
        m_Translation = dir * len;
    }
}

}