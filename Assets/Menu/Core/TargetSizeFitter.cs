using UnityEngine;
using UnityEngine.EventSystems;

namespace Discone.Ui {

/// resize the rect based as a percentage of the target size
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
sealed class TargetSizeFitter: UIBehaviour {
    // -- types --
    /// the target picking mode
    enum TargetMode {
        Any,
        Content
    }

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the target to inherit size from")]
    [SerializeField] RectTransform m_Target;

    [Tooltip("how the target is chosen")]
    [SerializeField] TargetMode m_TargetMode;

    [Tooltip("the percent size of the target; a negative value ignores the axis")]
    [SerializeField] Vector2 m_Percent = new Vector2(-1.0f, -1.0f);

    // -- props --
    /// the target rect to listen to changes from, if any
    TargetRect m_TargetRect;

    /// a dispose bag
    Subscriptions m_Subscriptions;

    // -- lifecycle --
    override protected void Start() {
        base.Start();

        // if missing, infer a target from the lone content element
        if (m_Target == null && m_TargetMode == TargetMode.Content) {
            m_Target = FindContent();
        }

        // set initial size
        Resize();

        // subscribe to updates
        m_TargetRect = m_Target.GetComponent<TargetRect>();
        if (m_TargetRect != null) {
            m_TargetRect.Changed += Resize;
        }
    }

    #if UNITY_EDITOR
    void Update() {
        Editor_Update();
    }
    #endif

    override protected void OnDestroy() {
        // unsubscribe
        if (m_TargetRect != null) {
            m_TargetRect.Changed -= Resize;
        }
    }

    // -- commands --
    /// resize based on the target size
    [ContextMenu("Resize")]
    void Resize() {
        var rect = transform as RectTransform;
        var size = rect.rect.size;
        var target = m_Target.rect.size;

        // calculate next size
        if (m_Percent.x >= 0.0f) {
            var width = target.x * m_Percent.x;
            if (width != size.x) {
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
        }

        if (m_Percent.y >= 0.0f) {
            var height = target.y * m_Percent.y;
            if (height != size.y) {
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            }
        }
    }

    // -- queries --
    /// find the lone content element
    RectTransform FindContent() {
        // find the child
        var t = transform;
        var n = t.childCount;
        if (n != 1) {
            Debug.LogError($"[menuuu] target size fitter must have exactly one content element");
        }

        var content = t.GetChild(0) as RectTransform;
        if (content == null) {
            Debug.LogError($"[menuuu] target size fitter must have a rect transform as content");
        }

        return content;
    }

    // -- editor --
    #if UNITY_EDITOR
    /// the debug target size
    Vector2 m_TargetSize;

    // -- d/lifecycle
    void Editor_Update() {
        // if running game, don't do anything
        if (Application.IsPlaying(gameObject)) {
            return;
        }

        // don't infer target in prefab mode
        var preview = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        if (preview == null && m_Target == null && m_TargetMode == TargetMode.Content) {
            m_Target = FindContent();
        }

        // if no target, do nothing
        if (m_Target == null) {
            return;
        }

        // if size is the same, don't do anything
        var size = m_Target.rect.size;
        if (size == m_TargetSize) {
            return;
        }

        // otherwise, resize
        Resize();
        m_TargetSize = size;
    }
    #endif
}

}