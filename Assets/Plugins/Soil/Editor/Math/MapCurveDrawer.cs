using UnityEngine;
using UnityEditor;

using E = UnityEditor.EditorGUI;
using U = UnityEditor.EditorGUIUtility;

namespace Soil.Editor {

[CustomPropertyDrawer(typeof(MapCurve))]
public sealed class MapCurveDrawer: PropertyDrawer {
    // -- constants --
    /// the width of the curve
    const float k_CurveWidth = 40f;

    // -- commands --
    public override void OnGUI(Rect r, SerializedProperty prop, GUIContent label) {
        E.BeginProperty(r, label, prop);

        // get attrs
        var curve = prop.FindPropertyRelative(nameof(MapCurve.Curve));
        var src = prop.FindPropertyRelative(nameof(MapCurve.Src));
        var dst = prop.FindPropertyRelative(nameof(MapCurve.Dst));

        // draw label w/ indent
        E.LabelField(r, label);

        // reset indent so that it doesn't affect inline fields
        var indent = E.indentLevel;
        E.indentLevel = 0;

        // move rect past the label
        var lw = U.labelWidth + Theme.Gap1;
        r.x += lw;
        r.width -= lw;

        // calculate the range width
        var rw = (r.width - k_CurveWidth - Theme.Gap3 * 2) / 2;

        // draw the src range
        var rr1 = r;
        rr1.width = rw;
        FloatRangeDrawer.DrawInput(rr1, src);

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
        FloatRangeDrawer.DrawInput(rr2, dst);

        // reset indent level
        E.indentLevel = indent;

        E.EndProperty();
    }
}

}