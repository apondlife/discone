using UnityEngine;
using UnityEditor;

using E = UnityEditor.EditorGUI;
using U = UnityEditor.EditorGUIUtility;

namespace Soil.Editor {

// TODO: show the current time when the game is playing
[CustomPropertyDrawer(typeof(EaseTimer))]
sealed class EaseTimerDrawer: PropertyDrawer {
    // -- constants --
    /// the width of the curve
    const float k_CurveWidth = 40f;

    // -- commands --
    public override void OnGUI(Rect r, SerializedProperty prop, GUIContent label) {
        E.BeginProperty(r, label, prop);

        // get attrs
        var curve = prop.FindPropertyRelative("m_Curve");
        var value = prop.FindPropertyRelative("m_Duration");

        // draw label w/ indent
        E.LabelField(r, label);

        // reset indent so that it doesn't affect inline fields
        var indent = E.indentLevel;
        E.indentLevel = 0;

        // move rect past the label
        var lw = U.labelWidth + Theme.Gap1;
        r.x += lw;
        r.width -= lw;

        // draw the curve
        var rc = r;
        rc.width = k_CurveWidth;
        rc.y -= 1;
        rc.height += 1;
        curve.animationCurveValue = E.CurveField(rc, curve.animationCurveValue);

        // draw the duration
        var delta = rc.width + Theme.Gap3;
        r.x += delta;
        r.width -= delta;
        value.floatValue = E.FloatField(r, value.floatValue);

        // reset indent level
        E.indentLevel = indent;

        E.EndProperty();
    }
}

}