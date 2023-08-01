using UnityEngine;
using UnityEditor;

/// shows shader vector prop as a vector2
public class ShowAsVector2Drawer: MaterialPropertyDrawer {
    // -- PropertyDrawer --
    public override void OnGUI(
        Rect rect,
        MaterialProperty prop,
        GUIContent label,
        MaterialEditor editor
    ) {
        if (prop.type != MaterialProperty.PropType.Vector) {
            editor.DefaultShaderProperty(prop, label.text);
        }
        else {
            // capture row width and update field width
            var width = rect.width;
            rect.width = 312.0f;

            // do something
            EditorGUIUtility.labelWidth = 0.0f;
            EditorGUIUtility.fieldWidth = 0.0f;

            if (!EditorGUIUtility.wideMode) {
                EditorGUIUtility.wideMode = true;
                EditorGUIUtility.labelWidth = width - rect.width;
            }

            // who knows
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;

            // update value
            var val = EditorGUI.Vector2Field(rect, label, prop.vectorValue);
            if (EditorGUI.EndChangeCheck()) {
                prop.vectorValue = val;
            }
        }
    }
}