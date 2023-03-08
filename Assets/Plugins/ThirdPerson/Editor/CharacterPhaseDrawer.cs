using UnityEngine;
using UnityEditor;

namespace ThirdPerson.Editor {

[CustomPropertyDrawer(typeof(Phase))]
public sealed class CharacterPhaseDrawer: PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.LabelField(position, "Phase: " + property.FindPropertyRelative("Name").stringValue);
        EditorGUI.EndProperty();
    }
}

}