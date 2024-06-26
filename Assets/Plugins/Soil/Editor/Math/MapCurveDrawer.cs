using UnityEngine;
using UnityEditor;

using E = UnityEditor.EditorGUI;
using U = UnityEditor.EditorGUIUtility;

namespace Soil.Editor {

// TODO: update layout to [curve] [src] ">" [dst]
[CustomPropertyDrawer(typeof(MapCurve))]
public sealed class MapCurveDrawer: PropertyDrawer {
    // -- constants --
    /// the width of the curve
    const float k_CurveWidth = 40f;

    // -- commands --
    public override void OnGUI(Rect r, SerializedProperty prop, GUIContent label) {
        E.BeginProperty(r, label, prop);

        // get attrs
        var src = prop.FindProp(nameof(MapCurve.Src));
        var dst = prop.FindProp(nameof(MapCurve.Dst));
        var curve = prop.FindProp(nameof(MapCurve.Curve));

        // draw label w/ indent
        E.LabelField(r, label);

        // reset indent so that it doesn't affect inline fields
        var indent = E.indentLevel;
        E.indentLevel = 0;

        // move rect past the label
        var lw = U.labelWidth + Theme.Gap1;
        r.x += lw;
        r.width -= lw;

        // draw the input
        DrawInput(r, src, dst, curve);

        // reset indent level
        E.indentLevel = indent;

        E.EndProperty();
    }

    // -- commands --
    /// draw the input for a map curve
    public static void DrawInput(
        Rect r,
        SerializedProperty src,
        SerializedProperty dst,
        SerializedProperty curve
    ) {
        var srcMin = src.FindProp(nameof(FloatRange.Min));
        var srcMax = src.FindProp(nameof(FloatRange.Max));
        var dstMin = dst.FindProp(nameof(FloatRange.Min));
        var dstMax = dst.FindProp(nameof(FloatRange.Max));
        DrawInput(r, srcMin, srcMax, dstMin, dstMax, curve);
    }

    /// draw the input for a map curve
    public static void DrawInput(
        Rect r,
        SerializedProperty srcMin,
        SerializedProperty srcMax,
        SerializedProperty dstMin,
        SerializedProperty dstMax,
        SerializedProperty curve
    ) {
        // calculate the range width
        var rw = (r.width - k_CurveWidth - Theme.Gap3 * 2) / 2;

        // draw the src range
        var rr1 = r;
        rr1.width = rw;
        FloatRangeDrawer.DrawInput(rr1, srcMin, srcMax);

        // move past the range
        r.x += rr1.width + Theme.Gap3;

        // draw the curve
        var rc = r;
        rc.width = k_CurveWidth;
        rc.y -= 1;
        rc.height += 1;
        curve.animationCurveValue = E.CurveField(rc, curve.animationCurveValue);

        // move past the curve
        r.x += rc.width + Theme.Gap3;

        // draw the dst range
        var rr2 = r;
        rr2.width = rw;
        FloatRangeDrawer.DrawInput(rr2, dstMin, dstMax);
    }
}

}