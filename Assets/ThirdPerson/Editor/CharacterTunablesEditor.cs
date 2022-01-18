using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterTunables))]
[CanEditMultipleObjects]
public class LookAtPointEditor: Editor {
    // -- fields --
    private CharacterTunables m_Tunables;

    // -- lifecycle --
    void OnEnable() {
        m_Tunables = serializedObject.targetObject as CharacterTunables;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        EditorGUILayout.LabelField("Acceleration: " + m_Tunables.Acceleration);
        EditorGUILayout.LabelField("Deceleration: " + m_Tunables.Deceleration);
        EditorGUILayout.LabelField("Jump Height: " + m_Tunables.JumpHeight);
        EditorGUILayout.LabelField("Jump Duration: " + m_Tunables.JumpDuration);
        EditorGUILayout.LabelField("Pivot Decleration: " + m_Tunables.PivotDeceleration);
    }
}