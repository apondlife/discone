using UnityEngine;
using UnityEditor;

using E = UnityEditor.EditorGUI;
using U = UnityEditor.EditorGUIUtility;

namespace Soil.Editor {

[CustomPropertyDrawer(typeof(EaseTimer.Config))]
sealed class EaseTimerConfigDrawer: PropertyDrawer {
    // -- PropertyDrawer --
    public override void OnGUI(Rect r, SerializedProperty prop, GUIContent label) {
        E.BeginProperty(r, label, prop);

        // draw label w/ indent
        E.LabelField(r, label);

        // reset indent so that it doesn't affect inline fields
        var indent = E.indentLevel;
        E.indentLevel = 0;

        // move rect past the label
        var lw = U.labelWidth + Theme.Gap1;
        r.x += lw;
        r.width -= lw;

        // draw the config
        DrawInput(r, prop);

        // reset indent level
        E.indentLevel = indent;

        E.EndProperty();
    }

    // -- commands --
    /// draw the config fields
    public static void DrawInput(Rect r, SerializedProperty prop) {
        // get attrs
        var value = prop.FindProp(nameof(EaseTimer.Config.Duration));
        var curve = prop.FindProp(nameof(EaseTimer.Config.Curve));

        // draw the curve
        Draw.CurveField(ref r, curve);

        // draw the duration
        var units = value.FindAttribute<UnitsAttribute>();
        Draw.FloatRangeField(r, null, max: value, units);
    }
}

}