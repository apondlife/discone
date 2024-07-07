using UnityEngine;
using UnityEditor;

using E = UnityEditor.EditorGUI;
using U = UnityEditor.EditorGUIUtility;

namespace Soil.Editor {

// TODO: update layout to [curve] [src] ">" [dst]
[CustomPropertyDrawer(typeof(MapCurve))]
public sealed class MapCurveDrawer: PropertyDrawer {
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
        var (srcUnits, dstUnits) = FindUnits(prop);
        DrawInput(r, src, srcUnits, dst, dstUnits, curve);

        // reset indent level
        E.indentLevel = indent;

        E.EndProperty();
    }

    // -- commands --
    /// draw the input for a map curve
    public static void DrawInput(
        Rect r,
        SerializedProperty src,
        UnitsAttribute srcUnits,
        SerializedProperty dst,
        UnitsAttribute dstUnits,
        SerializedProperty curve
    ) {
        var srcMin = src.FindProp(nameof(FloatRange.Min));
        var srcMax = src.FindProp(nameof(FloatRange.Max));

        var dstMin = dst.FindProp(nameof(FloatRange.Min));
        var dstMax = dst.FindProp(nameof(FloatRange.Max));

        DrawInput(r, srcMin, srcMax, srcUnits, dstMin, dstMax, dstUnits, curve);
    }

    /// draw the input for a map curve
    public static void DrawInput(
        Rect r,
        SerializedProperty srcMin,
        SerializedProperty srcMax,
        UnitsAttribute srcUnits,
        SerializedProperty dstMin,
        SerializedProperty dstMax,
        UnitsAttribute dstUnits,
        SerializedProperty curve
    ) {
        // draw the curve
        Draw.CurveField(ref r, curve);

        // calculate the range width
        var rw = (r.width - Theme.Gap3) / 2;

        // draw the src range
        var rr1 = r;
        rr1.width = rw;
        Draw.FloatRangeField(rr1, srcMin, srcMax, srcUnits);

        // move past the range
        r.x += rr1.width + Theme.Gap3;

        // draw the dst range
        var rr2 = r;
        rr2.width = rw;
        Draw.FloatRangeField(rr2, dstMin, dstMax, dstUnits);
    }

    // -- queries --
    /// find the src & dst units attached to a given property
    public static (UnitsAttribute, UnitsAttribute) FindUnits(SerializedProperty prop) {
        var srcUnits = null as UnitsAttribute;
        var dstUnits = null as UnitsAttribute;

        var unitsList = prop.FindAttributes<UnitsAttribute>();
        foreach (var units in unitsList) {
            if (units.For.HasFlag(UnitsFor.Src)) {
                srcUnits = units;
            }

            if (units.For.HasFlag(UnitsFor.Dst)) {
                dstUnits = units;
            }
        }

        return (srcUnits, dstUnits);
    }
}

}