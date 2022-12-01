using UnityEngine;
using UnityEditor;
using UnityAtoms;

namespace Discone.Editor {

/// a base class for selection editors
public sealed class Shortcuts: EditorWindow {
    // -- props --
    /// the editor title
    const string k_Title = "shortcuts";

    // -- fields --
    [Tooltip("the entity repos")]
    [SerializeField] EntitiesVariable m_Entities;

    // -- props --
    /// the search query
    string m_Query = "";

    // -- lifecycle --
    /// show the window
    [MenuItem("GameObject/discone/shortcuts")]
    static void Init() {
        var window = GetWindow<Shortcuts>(
            title: k_Title,
            focus: true
        );

        window.Show();
    }

    void OnGUI() {
        var entities = m_Entities.Value;

        // find current character
        var character = entities?
            .Players
            .Current?
            .Character;

        // show ui
        DrawCharacterView(character);
    }

    // -- ui --
    /// draw the character shortcuts
    void DrawCharacterView(DisconeCharacter character) {
        EditorGUILayout.BeginVertical();

        // show inspect shortcuts
        EditorGUILayout.LabelField(
            "current character",
            EditorStyles.boldLabel
        );

        // if character missing, game is probably not running
        if (character == null) {
            EditorGUILayout.LabelField("no current character set");
        }
        // show the character ui
        else {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();

            // show query ui
            GUILayout.Space(5f);
            DrawChildQuery();
            GUILayout.Space(7f);

            // select the character
            if (GUILayout.Button("select it", GUILayout.ExpandWidth(false))) {
                SelectCharacter(character);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            var ch = new SerializedObject(character);
            var state = ch.FindProperty("m_RemoteState");
            EditorGUILayout.PropertyField(state);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    /// show input field to query child obj of character
    void DrawChildQuery() {
        GUILayout.Label("child query");

        EditorGUILayout.BeginHorizontal();

        m_Query = GUILayout.TextField(
            m_Query,
            GUILayout.ExpandWidth(false),
            GUILayout.MinWidth(200.0f)
        );

        if (m_Query != "") {
            GUILayout.Space(5f);

            if (GUILayout.Button("x", GUILayout.ExpandWidth(false))) {
                ClearQuery();
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    // -- commands --
    /// clear query
    void ClearQuery() {
        m_Query = "";
    }

    /// select the character or child
    void SelectCharacter(DisconeCharacter character) {
        if (character == null) {
            return;
        }

        var selection = character.transform;
        if (m_Query != "") {
            foreach (var child in selection.GetComponentsInChildren<Transform>()) {
                if (child.name.Contains(m_Query)) {
                    selection = child;
                }
            }
        }

        // select their character
        Selection.activeGameObject = selection.gameObject;
    }

    // -- queries --
    /// build the path from the transform
    string PathFrom(Transform t) {
        if (t.parent == null) {
            return t.name;
        }

        return $"{PathFrom(t.parent)}/{t.name}";
    }
}

}