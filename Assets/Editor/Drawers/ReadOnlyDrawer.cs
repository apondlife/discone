using UnityEngine;
using UnityEditor;

/// shows field as readonly
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer {
    // -- PropertyDrawer --
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        var prev = GUI.enabled;
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = prev;
    }
}