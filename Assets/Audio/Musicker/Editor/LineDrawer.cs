using UnityEngine;
using UnityEditor;

namespace Musicker {

[CustomPropertyDrawer(typeof(LineField))]
sealed class LineDrawer: PropertyDrawer {
    // -- lifecycle --
    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent name) {
        // get the prop by reflection
        var notation = prop.FindPropertyRelative("m_Notation");

        // render drawer
        EditorGUI.BeginProperty(pos, name, prop);
        var label = EditorGUI.PrefixLabel(pos, name);
        var field = EditorGUI.TextField(label, notation.stringValue);
        EditorGUI.EndProperty();

        // update notation
        notation.stringValue = field;
    }
}

}