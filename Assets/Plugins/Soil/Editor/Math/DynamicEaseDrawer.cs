using UnityEditor;
using UnityEngine;
using E = UnityEditor.EditorGUI;
using U = UnityEditor.EditorGUIUtility;

namespace Soil.Editor {

[CustomPropertyDrawer(typeof(DynamicEase))]
public class DynamicEaseDrawer: PropertyDrawer {
    // -- constants --
    /// the width of a Vector3 drawer label
    const float k_LabelWidth = 15.5f;

    /// a small inset on the Vector3 drawer (it doesn't actually reach the right edge)
    const float k_RightInset = 0.5f;

    // -- PropertyDrawer --
    public override void OnGUI(Rect r, SerializedProperty prop, GUIContent label) {
        E.BeginProperty(r, label, prop);

        // get attrs
        var pF = prop.FindPropertyRelative("F");
        var pZ = prop.FindPropertyRelative("Z");
        var pR = prop.FindPropertyRelative("R");

        // draw label w/ indent
        E.LabelField(r, label);

        // move rect past the label
        var lw = U.labelWidth + Theme.Gap1;
        r.x += lw;
        r.width -= lw;

        var fw = (r.width - k_RightInset - k_LabelWidth * 3f - Theme.Gap2 * 2f) / 3f;
        var fr = r;
        fr.width = fw;

        pF.floatValue = DrawInput(fr, fw, pF);
        fr.x += fw + k_LabelWidth + Theme.Gap2;
        pZ.floatValue = DrawInput(fr, fw, pZ);
        fr.x += fw + k_LabelWidth + Theme.Gap2;
        pR.floatValue = DrawInput(fr, fw, pR);

        E.EndProperty();
    }

    float DrawInput(Rect r, float w, SerializedProperty prop) {
        r.width = k_LabelWidth;
        E.LabelField(r, prop.name);

        r.x += k_LabelWidth;
        r.width = w;
        var newValue = E.FloatField(r, prop.floatValue);

        return newValue;
    }
}

}