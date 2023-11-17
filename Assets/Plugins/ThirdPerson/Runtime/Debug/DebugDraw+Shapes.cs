using UnityEngine;
using System.Collections.Generic;
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

        using (Draw.Command(cam)) {
            Draw.LineGeometry = LineGeometry.Volumetric3D;
            Draw.ThicknessSpace = ThicknessSpace.Pixels;

            // draw lines
            foreach (var value in m_Values) {
                if (!value.IsEnabled) {
                    continue;
                }

                for (var i = 0u; i < value.Count; i++) {
                    var ray = value[i];

                    Draw.Line(
                        ray.Pos,
                        ray.Pos + m_LengthScale * ray.Dir,
                        value.Width * m_WidthScale,
                        value.Color
                    );
                }
            }
        }
    }
}

}