using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Field))]
public class FieldEditor: Editor {
    // -- props --
    /// the field
    private Field m_Field;

    // -- lifecycle --
    void OnEnable() {
        m_Field = serializedObject.targetObject as Field;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        // draw clear button
        if (GUILayout.Button("Clear")) {
            m_Field.ClearEditorChunks();
        }
    }
}
