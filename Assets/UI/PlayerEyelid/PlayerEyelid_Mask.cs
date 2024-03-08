using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.UI;

namespace Discone.Ui {

/// the mask shape for the player eye
public class PlayerEyelid_Mask: MaskableGraphic {
    // -- refs --
    [Header("refs")]
    [Tooltip("the current eyelid close pct")]
    [SerializeField] FloatVariable m_ClosePct;

    [Tooltip("if the eyelid is always open")]
    [SerializeField] BoolVariable m_IsAlwaysOpen;

    // -- props --
    /// the list of subscriptions
    DisposeBag m_Subscriptions = new();

    // -- lifecycle --
    protected override void Start() {
        base.Start();

        // bind events
        m_Subscriptions
            .Add(m_ClosePct, OnClosePctChanged)
            .Add(m_IsAlwaysOpen, OnIsAlwaysOpenChanged);
    }

    protected override void OnDestroy() {
        m_Subscriptions.Dispose();
        base.OnDestroy();
    }

    // -- events --
    void OnClosePctChanged(float _) {
        SetVerticesDirty();
    }

    void OnIsAlwaysOpenChanged(bool _) {
        SetVerticesDirty();
    }

    // -- MaskableGraphic --
    protected override void OnPopulateMesh(VertexHelper vh) {
        base.OnPopulateMesh(vh);

        // get rect
        var r = rectTransform.rect;
        var w = r.width;
        var w2 = w * 0.5f;
        var h = r.height;
        var h2 = h * 0.5f;

        // get close percent
        var pct = 1f;
        if (m_IsAlwaysOpen == null || !m_IsAlwaysOpen.Value) {
            pct = m_ClosePct.Value;
        }

        // get lid height
        var hLid = h2 * pct;

        // draw top eyelid
        var y0 = h2;
        var y1 = h2 - hLid;
        vh.AddUIVertexQuad(new[] {
            Point(x: -w2, y: y0),
            Point(x: -w2, y: y1),
            Point(x: +w2, y: y1),
            Point(x: +w2, y: y0)
        });

        // draw bottom eyelid
        y0 = -h2 + hLid;
        y1 = -h2;
        vh.AddUIVertexQuad(new[] {
            Point(x: -w2, y: y0),
            Point(x: -w2, y: y1),
            Point(x: +w2, y: y1),
            Point(x: +w2, y: y0)
        });
    }

    // -- queries --
    /// create a vert w/ the point
    static UIVertex Point(float x, float y) {
        var vert = UIVertex.simpleVert;
        vert.position = new Vector3(x, y, 0f);
        vert.color = Color.green;
        return vert;
    }
}

}