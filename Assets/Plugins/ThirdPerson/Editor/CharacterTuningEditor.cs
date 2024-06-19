using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Soil;
using UnityEditor;
using UnityEngine;
using E = UnityEditor.EditorGUI;
using L = UnityEditor.EditorGUILayout;
using G = UnityEngine.GUILayout;

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

    /// if the foldout by name is expanded
    Dictionary<string, bool> m_IsFoldoutExpanded = new();

    // -- lifecycle --
    void OnEnable() {
        m_Tuning = serializedObject.targetObject as CharacterTuning;

        // set statics
        if (s_NamePattern == null) {
            s_NamePattern = new Regex("(\\B[A-Z])", RegexOptions.Compiled);
        }
    }

    public override void OnInspectorGUI() {
        // show buttons
        if (G.Button("sync all")) {
            SyncAll();
        }

        // show developer description
        Field(serializedObject.FindProperty("m_Description"));

        // show tuning properties
        var type = m_Tuning.GetType();

        var members = type
            .GetFields()
            .Concat(type.GetProperties().Cast<MemberInfo>());

        var currFoldout = null as string;
        foreach (var m in members) {
            // skip members from supertypes
            if (m.DeclaringType != type) {
                continue;
            }

            var foldout = m.GetCustomAttribute<FoldoutAttribute>();
            if (foldout != null) {
                if (currFoldout != null) {
                    L.EndFoldoutHeaderGroup();
                    E.indentLevel -= 1;
                    currFoldout = null;
                }

                currFoldout = foldout.Name;
                m_IsFoldoutExpanded[currFoldout] = L.BeginFoldoutHeaderGroup(
                    m_IsFoldoutExpanded.GetValueOrDefault(currFoldout),
                    new GUIContent(foldout.Name)
                );

                E.indentLevel += 1;
            }

            if (currFoldout == null || !m_IsFoldoutExpanded.GetValueOrDefault(currFoldout)) {
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
        L.PropertyField(prop);
    }

    void Row(string label, object value) {
        L.BeginHorizontal();
        L.PrefixLabel(label);
        L.LabelField(value.ToString());
        L.EndHorizontal();
    }

    static void SyncAll() {
        var guids = AssetDatabase.FindAssets($"t:{nameof(CharacterTuning)}");
        foreach (var guid in guids) {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<CharacterTuning>(assetPath);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssetIfDirty(asset);
        }

        AssetDatabase.Refresh();
    }
}

}