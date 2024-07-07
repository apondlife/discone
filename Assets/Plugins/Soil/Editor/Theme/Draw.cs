using UnityEngine;
using UnityEditor;

using E = UnityEditor.EditorGUI;

namespace Soil.Editor {

static class Draw {
    // -- constants --
    /// the width of the curve
    const float k_CurveWidth = 40f;

    /// the width of the range separator label ("...")
    const float k_SeparatorWidth = 16f;

    /// the separator style
    static GUIStyle s_Separator;

    // -- commands --
    /// draw a a curve and advance the rect past the curve
    public static void CurveField(
        ref Rect r,
        SerializedProperty curve
    ) {
        var rc = r;

        // draw the curve
        rc.width = k_CurveWidth;
        rc.y -= 1;
        rc.height += 1;
        curve.animationCurveValue = E.CurveField(rc, curve.animationCurveValue);

        // move past the curve
        var delta = rc.width + Theme.Gap3;
        r.x += delta;
        r.width -= delta;
    }

    /// draw the range input
    public static void FloatRangeField(
        Rect r,
        SerializedProperty min,
        SerializedProperty max,
        UnitsAttribute units = null
    ) {
        var isRange = min != null && max != null;

        // get the width of the labels
        var lw = 0f;
        if (isRange) {
            lw += k_SeparatorWidth + Theme.Gap1;
        }

        // get the width of the units label
        var uw = 0f;
        if (units != null) {
            uw += UnitsStyle().CalcSize(new GUIContent(units.Label)).x;
            lw += uw + Theme.Gap1;
        }

        // calc width of each field from remaining space
        var fw = r.width - lw;
        if (isRange) {
            fw = (fw - Theme.Gap1) / 2;
        }

        // draw the min field
        if (min != null) {
            r.width = fw;
            min.floatValue = E.FloatField(r, min.floatValue);
            r.x += fw + Theme.Gap1;
        }

        // draw the separator
        if (isRange) {
            r.width = k_SeparatorWidth;
            E.LabelField(r, "...", SeparatorStyle());
            r.x += k_SeparatorWidth + Theme.Gap1;
        }

        // draw the max field
        if (max != null) {
            r.width = fw;
            max.floatValue = E.FloatField(r, max.floatValue);
        }

        // draw the units
        if (units != null) {
            r.x += fw + Theme.Gap1;
            r.width = uw;
            E.LabelField(r, units.Label, UnitsStyle());
        }
    }

    // -- queries --
    /// the separator text style
    static GUIStyle SeparatorStyle() {
        if (s_Separator != null) {
            return s_Separator;
        }

        var separator = new GUIStyle(GUI.skin.label);
        separator.alignment = TextAnchor.MiddleCenter;
        s_Separator = separator;

        return separator;
    }

    /// the units text style
    static GUIStyle UnitsStyle() {
        return GUI.skin.label;
    }
}

}