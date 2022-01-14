using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterTunables))]
[CanEditMultipleObjects]
public class LookAtPointEditor: Editor {
    private CharacterTunables reference;

    void OnEnable() {
        reference = serializedObject.targetObject as CharacterTunables;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        EditorGUILayout.LabelField("Acceleration: " + reference.Acceleration);
        EditorGUILayout.LabelField("Deceleration: " + reference.Deceleration);
    }
}