using Soil.Editor;
using UnityEngine;
using UnityEditor;

using E = UnityEditor.EditorGUI;
using U = UnityEditor.EditorGUIUtility;

namespace Soil.Editor {

[CustomPropertyDrawer(typeof(MapInCurve))]
sealed class MapInCurveDrawer: PropertyDrawer {
    // -- constants --
    /// the width of the curve
    const float k_CurveWidth = 40f;

    // -- commands --
    public override void OnGUI(Rect r, SerializedProperty prop, GUIContent label) {
        E.BeginProperty(r, label, prop);

        // get attrs
        var src = prop.FindProp(nameof(MapInCurve.Src));
        var curve = prop.FindProp(nameof(MapInCurve.Curve));

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
        DrawInput(r, src, curve);

        // reset indent level
        E.indentLevel = indent;

        E.EndProperty();
    }

    // -- commands --
    /// draw the input for a map in curve
    public static void DrawInput(
        Rect r,
        SerializedProperty src,
        SerializedProperty curve
    ) {
        var srcMin = src.FindProp(nameof(FloatRange.Min));
        var srcMax = src.FindProp(nameof(FloatRange.Max));
        DrawInput(r, srcMin, srcMax, curve);
    }

    /// draw the input for a map in curve
    public static void DrawInput(
        Rect r,
        SerializedProperty srcMin,
        SerializedProperty srcMax,
        SerializedProperty curve
    ) {
        // draw the curve
        var rc = r;
        rc.width = k_CurveWidth;
        rc.y -= 1;
        rc.height += 1;
        curve.animationCurveValue = E.CurveField(rc, curve.animationCurveValue);

        // draw the range
        var delta = rc.width + Theme.Gap3;
        r.x += delta;
        r.width -= delta;
        FloatRangeDrawer.DrawInput(r, srcMin, srcMax);
    }
}

}