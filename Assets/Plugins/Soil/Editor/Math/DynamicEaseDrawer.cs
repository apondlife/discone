using UnityEditor;
using UnityEngine;
using E = UnityEditor.EditorGUI;
using U = UnityEditor.EditorGUIUtility;

namespace Soil.Editor {

[CustomPropertyDrawer(typeof(DynamicEase))]
public class DynamicEaseDrawer: PropertyDrawer {
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

        // draw fzr fields
        var labels = new GUIContent[] { new(pF.name), new(pZ.name), new(pR.name) };
        var values = new[] { pF.floatValue, pZ.floatValue, pR.floatValue };
        E.MultiFloatField(r, labels, values);

        pF.floatValue = values[0];
        pZ.floatValue = values[1];
        pR.floatValue = values[2];

        E.EndProperty();
    }
}

}