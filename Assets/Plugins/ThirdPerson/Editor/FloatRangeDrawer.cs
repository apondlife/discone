using UnityEngine;
using UnityEditor;

using E = UnityEditor.EditorGUI;
using U = UnityEditor.EditorGUIUtility;

namespace ThirdPerson.Editor {

[CustomPropertyDrawer(typeof(FloatRange))]
sealed class FloatRangeDrawer: PropertyDrawer {
    // -- constants --
    /// the gap between elements
    const float k_Gap = 2f;

    /// the width of the range separator label ("...")
    const float k_SeparatorWidth = 16f;

    /// the separator style
    static GUIStyle s_Separator;

    // -- lifecycle --
    public override void OnGUI(Rect r, SerializedProperty prop, GUIContent label) {
        E.BeginProperty(r, label, prop);

        // draw label w/ indent
        E.LabelField(r, label);

        // reset indent so that it doesn't affect inline fields
        var indent = E.indentLevel;
        E.indentLevel = 0;

        // move rect past the label
        var lw = U.labelWidth + k_Gap;
        r.x += lw;
        r.width -= lw;

        // draw the range input
        DrawInput(r, prop);

        // reset indent level
        E.indentLevel = indent;

        E.EndProperty();
    }

    // -- commands --
    /// draw the range input
    public static void DrawInput(Rect r, SerializedProperty prop) {
        var min = prop.FindPropertyRelative("Min");
        var max = prop.FindPropertyRelative("Max");

        // calc width of each field from remaining space
        var fw = (r.width - k_SeparatorWidth - k_Gap * 2f) / 2f;

        // draw the min field
        r.width = fw;
        min.floatValue = E.FloatField(r, min.floatValue);
        r.x += fw + k_Gap;

        // draw the separator
        r.width = k_SeparatorWidth;
        E.LabelField(r, "...", Separator());
        r.x += k_SeparatorWidth + k_Gap;

        // draw the max field
        r.width = fw;
        max.floatValue = E.FloatField(r, max.floatValue);
    }

    // -- queries --
    /// the separator
    static GUIStyle Separator() {
        if (s_Separator != null) {
            return s_Separator;
        }

        var separator = new GUIStyle(GUI.skin.label);
        separator.alignment = TextAnchor.MiddleCenter;
        s_Separator = separator;

        return separator;
    }
}

}