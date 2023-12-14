using System;
using UnityEngine;
using Shapes;

namespace  ThirdPerson {

/// a debug utility for adding drawings
public partial class DebugDraw {
    // -- drawing --
    [Header("drawing")]
    [Tooltip("the scaling factor for ray thickness")]
    [SerializeField] float m_Width = 1f;

    [Tooltip("the scaling factor for ray length")]
    [SerializeField] float m_Scale = 1f;

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

                var i0 = Math.Max(m_Range.Min, value.Range.Min);
                var i1 = Math.Min(m_Range.Max, value.Range.Max);
                for (var i = i0; i < i1; i++) {
                    var ray = value[i];
                    var color = value.Color.Evaluate(Mathf.InverseLerp(i0, i1, i));

                    // draw line
                    Draw.Line(
                        ray.Pos,
                        ray.Pos + m_Scale * value.Scale * ray.Dir,
                        value.Width * m_Width,
                        color
                    );
                }
            }
        }
    }
}

}