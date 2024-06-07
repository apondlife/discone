using System.Reflection;
using UnityEngine;
using UnityEditor;

using E = UnityEditor.EditorGUI;
using U = UnityEditor.EditorGUIUtility;

namespace Soil.Editor {

[CustomPropertyDrawer(typeof(IntRange))]
public sealed class IntRangeDrawer: PropertyDrawer {
    // -- constants --
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
        var lw = U.labelWidth + Theme.Gap1;
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

        // find the range attribute
        var objType = prop.serializedObject.targetObject.GetType();
        var rangeProp = objType.GetField(prop.name, BindingFlags.NonPublic | BindingFlags.Instance);
        var rangeAttr = rangeProp?.GetCustomAttribute(typeof(RangeAttribute)) as RangeAttribute;

        // calc width of each field from remaining space
        var sw = rangeAttr == null ? k_SeparatorWidth : 100f;
        var fw = (r.width - sw - Theme.Gap1 * 2f) / 2f;

        // the next min  & max
        var nextMin = min.intValue;
        var nextMax = max.intValue;

        // draw the min field
        r.width = fw;
        nextMin = E.DelayedIntField(r, nextMin);
        r.x += fw + Theme.Gap1;

        // if this has range extents, draw the slider
        if (rangeAttr != null) {
            var minF = (float)nextMin;
            var maxF = (float)nextMax;

            r.width = sw;
            E.MinMaxSlider(r, ref minF, ref maxF, rangeAttr.Min, rangeAttr.Max);
            r.x += sw + Theme.Gap1;

            nextMin = (int)minF;
            nextMax = (int)maxF;
        }
        // otherwise, draw the separator
        else {
            r.width = sw;
            E.LabelField(r, "...", Separator());
            r.x += sw + Theme.Gap1;
        }

        // draw the max field
        r.width = fw;
        nextMax = E.DelayedIntField(r, nextMax);

        // update the clamped values
        if (min.intValue != nextMin) {
            min.intValue = nextMin;
            max.intValue = Mathf.Max(nextMin, nextMax);
        } else if (max.intValue != nextMax) {
            min.intValue = Mathf.Min(nextMin, nextMax);
            max.intValue = nextMax;
        }
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