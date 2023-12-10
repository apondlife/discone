using UnityEngine;
using Shapes;

namespace  ThirdPerson {

/// a debug utility for adding drawings
public partial class DebugDraw {
    // -- drawing --
    [Header("drawing")]
    [Tooltip("the scaling factor for drawn rays")]
    [SerializeField] float m_LengthScale;

    [Tooltip("the scaling factor for ray thickness")]
    [SerializeField] float m_WidthScale;

    // -- lifecycle --
    public override void DrawShapes(UnityEngine.Camera cam) {
        base.DrawShapes(cam);

        // only draw when enabled
        if (!m_IsEnabled) {
            return;
        }

        // draw every debug value
        using (Draw.Command(cam)) {
            Draw.LineGeometry = LineGeometry.Volumetric3D;
            Draw.ThicknessSpace = ThicknessSpace.Pixels;

            // draw lines
            foreach (var value in m_Values) {
                if (!value.IsEnabled) {
                    continue;
                }

                var color = value.Color;
                var maxAlpha = color.a;

                var n = value.Count;
                for (var i = 0u; i < n; i++) {
                    var ray = value[i];

                    // interpolate alpha to fade out older values
                    color.a = Mathf.Lerp(
                        maxAlpha,
                        value.MinAlpha,
                        (float)i / n
                    );

                    // draw line
                    Draw.Line(
                        ray.Pos,
                        ray.Pos + m_LengthScale * value.LengthScale * ray.Dir,
                        value.Width * m_WidthScale,
                        color
                    );
                }
            }
        }
    }
}

}