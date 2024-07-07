using UnityEngine;
using UnityEditor;

using E = UnityEditor.EditorGUI;
using U = UnityEditor.EditorGUIUtility;

namespace Soil.Editor {

[CustomPropertyDrawer(typeof(MapOutCurve))]
public sealed class MapOutCurveDrawer: PropertyDrawer {
    // -- commands --
    public override void OnGUI(Rect r, SerializedProperty prop, GUIContent label) {
        E.BeginProperty(r, label, prop);

        // get attrs
        var dst = prop.FindProp(nameof(MapOutCurve.Dst));
        var curve = prop.FindProp(nameof(MapOutCurve.Curve));

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
        var dstUnits = dst.FindAttribute<UnitsAttribute>();
        DrawInput(r, dst, dstUnits, curve);

        // reset indent level
        E.indentLevel = indent;

        E.EndProperty();
    }

    // -- commands --
    /// draw the input for a map out curve
    public static void DrawInput(
        Rect r,
        SerializedProperty dst,
        UnitsAttribute dstUnits,
        SerializedProperty curve
    ) {
        var dstMin = dst.FindProp(nameof(FloatRange.Min));
        var dstMax = dst.FindProp(nameof(FloatRange.Max));
        DrawInput(r, dstMin, dstMax, dstUnits, curve);
    }

    /// draw the input for a map out curve
    public static void DrawInput(
        Rect r,
        SerializedProperty dstMin,
        SerializedProperty dstMax,
        UnitsAttribute dstUnits,
        SerializedProperty curve
    ) {
        // draw the curve
        Draw.CurveField(ref r, curve);

        // draw the range
        Draw.FloatRangeField(r, dstMin, dstMax, dstUnits);
    }
}

}