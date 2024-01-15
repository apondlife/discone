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

    // -- lifecycle --
    void Update() {
        if (m_ClosePct.Value != m_ClosePct.OldValue) {
            SetVerticesDirty();
        }
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

        // get lid height
        var hLid = h2 * m_ClosePct.Value;

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