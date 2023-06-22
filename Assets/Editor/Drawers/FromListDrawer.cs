using UnityEngine;
using UnityEditor;

/// shows string field as a dropdown from a list of values
[CustomPropertyDrawer(typeof(FromListAttribute))]
public sealed class FromListDrawer: PropertyDrawer {
    // -- PropertyDrawer --
    public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label) {
        var list = (attribute as FromListAttribute).List;

        var index = EditorGUI.Popup(
            rect,
            prop.displayName,
            Mathf.Max(System.Array.IndexOf(list, prop.stringValue), 0),
            list
        );

        prop.stringValue = list[index];
    }
}
