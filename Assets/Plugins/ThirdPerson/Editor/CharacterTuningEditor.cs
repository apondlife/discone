using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;

namespace ThirdPerson.Editor {

[CustomEditor(typeof(CharacterTuning))]
[CanEditMultipleObjects]
sealed class CharacterTuningEditor: UnityEditor.Editor {
    // -- statics --
    /// a pattern to add spaces to property names
    static Regex s_NamePattern;

    // -- fields --
    /// the character tuning
    CharacterTuning m_Tuning;

    // -- lifecycle --
    void OnEnable() {
        m_Tuning = serializedObject.targetObject as CharacterTuning;

        // set statics
        if (s_NamePattern == null) {
            s_NamePattern = new Regex("(\\B[A-Z])", RegexOptions.Compiled);
        }
    }

    public override void OnInspectorGUI() {
        // show developer description
        Field(serializedObject.FindProperty("m_Description"));

        // show tuning properties
        var type = m_Tuning.GetType();

        var members = type
            .GetFields().Cast<MemberInfo>()
            .Concat(type.GetProperties().Cast<MemberInfo>());

        foreach (var m in members) {
            // skip members from supertypes
            if (m.DeclaringType != type) {
                continue;
            }

            // render serialized properties as fields
            if (m is FieldInfo) {
                Field(serializedObject.FindProperty(m.Name));
            }
            // render everything else as readonly
            else if (m is PropertyInfo p) {
                Row(s_NamePattern.Replace(m.Name, " $1"), p.GetValue(m_Tuning));
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
