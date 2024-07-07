using UnityEngine;
using UnityEditor;

using E = UnityEditor.EditorGUI;
using U = UnityEditor.EditorGUIUtility;

namespace Soil.Editor {

[CustomPropertyDrawer(typeof(MaxInCurve))]
sealed class MaxInCurveDrawer: PropertyDrawer {
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
        var srcUnits = src.FindAttribute<UnitsAttribute>();
        DrawInput(r, src, srcUnits, curve);

        // reset indent level
        E.indentLevel = indent;

        E.EndProperty();
    }

    // -- commands --
    /// draw the input for a map in max curve
    public static void DrawInput(
        Rect r,
        SerializedProperty srcMax,
        UnitsAttribute srcUnits,
        SerializedProperty curve
    ) {
        // draw the curve
        Draw.CurveField(ref r, curve);

        // draw the max input
        Draw.FloatRangeField(r, null, max: srcMax, srcUnits);
    }
}

}