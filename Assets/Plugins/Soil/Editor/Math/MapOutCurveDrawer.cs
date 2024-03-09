using UnityEngine;
using UnityEditor;

using E = UnityEditor.EditorGUI;
using U = UnityEditor.EditorGUIUtility;

namespace Soil.Editor {

[CustomPropertyDrawer(typeof(MapOutCurve))]
public sealed class MapOutCurveDrawer: PropertyDrawer {
    // -- constants --
    /// the gap between elements
    const float k_Gap1 = 2f;

    /// the gap between curve & field
    const float k_Gap2 = 6f;

    /// the width of the curve
    const float k_CurveWidth = 40f;

    // -- commands --
    public override void OnGUI(Rect r, SerializedProperty prop, GUIContent label) {
        E.BeginProperty(r, label, prop);

        // get attrs
        var curve = prop.FindPropertyRelative(nameof(MapOutCurve.Curve));
        var range = prop.FindPropertyRelative(nameof(MapOutCurve.Dst));

        // draw label w/ indent
        E.LabelField(r, label);

        // reset indent so that it doesn't affect inline fields
        var indent = E.indentLevel;
        E.indentLevel = 0;

        // move rect past the label
        var lw = U.labelWidth + k_Gap1;
        r.x += lw;
        r.width -= lw;

        // draw the curve
        var rc = r;
        rc.width = k_CurveWidth;
        rc.y -= 1;
        rc.height += 1;
        curve.animationCurveValue = E.CurveField(rc, curve.animationCurveValue);

        // draw the range
        var delta = rc.width + k_Gap2;
        r.x += delta;
        r.width -= delta;
        FloatRangeDrawer.DrawInput(r, range);

        // reset indent level
        E.indentLevel = indent;

        E.EndProperty();
    }
}

}