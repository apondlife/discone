using System;
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
            // select the character
            if (GUILayout.Button("select")) {
                SelectCharacter(character);
            }
        }

        EditorGUILayout.EndVertical();
    }

    // -- commands --
    /// select the character
    void SelectCharacter(DisconeCharacter character) {
        if (character == null) {
            return;
        }

        // select their character
        Selection.activeGameObject = character.gameObject;
    }
}

}