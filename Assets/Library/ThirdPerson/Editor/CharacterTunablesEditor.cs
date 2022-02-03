using System.Text.RegularExpressions;
using UnityEditor;

namespace ThirdPerson {

[CustomEditor(typeof(CharacterTunables))]
[CanEditMultipleObjects]
sealed class LookAtPointEditor: Editor {
    // -- statics --
    /// a pattern to add spaces to property names
    private static Regex s_NamePattern;

    // -- fields --
    /// the character tunables
    private CharacterTunables m_Tunables;

    // -- lifecycle --
    void OnEnable() {
        m_Tunables = serializedObject.targetObject as CharacterTunables;

        // set statics
        if (s_NamePattern == null) {
            s_NamePattern = new Regex("(\\B[A-Z])", RegexOptions.Compiled);
        }
    }

    public override void OnInspectorGUI() {
        // show developer description
        Field(serializedObject.FindProperty("m_Description"));

        // show tunable properties
        var type = m_Tunables.GetType();
        foreach (var p in type.GetProperties()) {
            // skip properties from supertypes
            if (p.DeclaringType != type) {
                continue;
            }

            var serialized = serializedObject.FindProperty($"m_{p.Name}");

            // render serialized properties as fields
            if (serialized != null) {
                Field(serialized);
            }
            // render everything else as readonly
            else {
                Row(s_NamePattern.Replace(p.Name, " $1"), p.GetValue(m_Tunables));
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    // -- commands --
    void Field(SerializedProperty prop) {
        EditorGUILayout.PropertyField(prop);
    }

    void Row(string label, object value) {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(label);
        EditorGUILayout.LabelField(value.ToString());
        EditorGUILayout.EndHorizontal();
    }
}

}
