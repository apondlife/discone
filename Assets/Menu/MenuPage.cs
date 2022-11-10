using UnityEngine;

/// a menu page
[ExecuteAlways]
sealed class MenuPage: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the elements on this page (set at runtime)")]
    [SerializeField] MenuElement[] m_Elements;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Elements = GetComponentsInChildren<MenuElement>();
    }

    // -- commands --
    /// show or hide the page
    public void Show(float pct) {
        foreach (var element in m_Elements) {
            Show(pct);
        }
    }
}