using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

[CustomEditor(typeof(CharacterTunables))]
[CanEditMultipleObjects]
public class LookAtPointEditor: Editor {
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
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Description"));

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
                EditorGUILayout.PropertyField(serialized);
            }
            // render everything else as readonly
            else {
                var label = s_NamePattern.Replace(p.Name, " $1");
                Row(label, p.GetValue(m_Tunables));
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    // -- commands --
    void Row(string label, object value) {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(label);
        EditorGUILayout.LabelField(value.ToString());
        EditorGUILayout.EndHorizontal();
    }
}