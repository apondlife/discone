using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.EventSystems;

/// resize the rect based as a percentage of the target size
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class TargetSizeFitter: UIBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the target to inherit size from")]
    [SerializeField] RectTransform m_Target;

    [Tooltip("the percent size of the target; a negative value ignores the axis")]
    [SerializeField] Vector2 m_Percent = new Vector2(-1.0f, -1.0f);

    // -- props --
    Subscriptions m_Subscriptions;

    /// our own rect transform
    RectTransform m_Rect;

    /// the target rect to listen to changes from, if any
    TargetRect m_TargetRect;

    // -- lifecycle --
    override protected void Awake() {
        base.Awake();

        // set props
        m_Rect = GetComponent<RectTransform>();

        // set initial size
        Resize();
    }

    override protected void Start() {
        base.Start();

        // subscribe to updates
        m_TargetRect = m_Target.GetComponent<TargetRect>();
        if (m_TargetRect != null) {
            m_TargetRect.Changed += Resize;
        }
    }

    override protected void OnDestroy() {
        // unsubscribe
        if (m_TargetRect != null) {
            m_TargetRect.Changed -= Resize;
        }
    }

    // -- commands --
    /// resize based on the target size
    void Resize() {
        var size = m_Rect.rect.size;
        var target = m_Target.rect.size;

        // calculate next size
        if (m_Percent.x >= 0.0f) {
            var width = target.x * m_Percent.x;
            if (width != size.x) {
                m_Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
        }

        if (m_Percent.y >= 0.0f) {
            var height = target.y * m_Percent.y;
            if (height != size.y) {
                m_Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            }
        }
    }
}
