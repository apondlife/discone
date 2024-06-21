using Soil;
using UnityEngine;
using UnityEditor;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using E = UnityEditor.EditorGUILayout;
using G = UnityEngine.GUILayout;

namespace Discone.Editor {

/// a base class for selection editors
public sealed class Shortcuts: EditorWindow {
    // -- constants --
    /// the editor title
    const string k_Title = "shortcuts";

    // -- fields --
    [Tooltip("the entity repos")]
    [SerializeField] EntitiesVariable m_Entities;

    [Tooltip("the start warp query")]
    [SerializeField] StringVariable m_StartWarp;

    // -- props --
    /// the current scroll position
    Vector2 m_ScrollPos;

    /// the search query
    string m_Query = "";

    /// the manually selected character
    Character m_Character;

    /// the repaint timer
    EaseTimer m_Repaint = new(1f / 60f);

    // -- lifecycle --
    /// show the window
    [MenuItem("Window/discone/shortcuts %#d")]
    static void Init() {
        var window = GetWindow<Shortcuts>(
            title: k_Title,
            focus: true
        );

        window.Show();
    }

    void OnGUI() {
        S.Init();

        var entities = m_Entities.Value;

        // find current character
        var character = entities?
            .OnlinePlayers
            .Current?
            .Character;

        // show ui
        DrawView(character);
    }

    void Start() {
        if (EditorApplication.isPlaying && !EditorApplication.isPaused) {
            m_Repaint.Start();
        }
    }

    void OnInspectorUpdate() {
        if (EditorApplication.isPlaying && !EditorApplication.isPaused) {
            Repaint();
        }
    }

    // -- ui --
    /// draw the character shortcuts
    void DrawView(Character character) {
        if (position.width >= (S.Margin + S.ColumnWidth) * 2 * S.Spacing) {
            DrawViewHorizontal(character);
        } else {
            DrawViewVertical(character);
        }
    }

    /// draw the vertical layout
    void DrawViewVertical(Character character) {
        m_ScrollPos = L.BS(m_ScrollPos);
            L.BV(S.Margins, G.MaxWidth(S.ColumnWidth));
                DrawStartWarpField();
                E.Space(S.Spacing, false);

                DrawCharacterSearch(character);
                E.Space(S.Spacing, false);

                // show the state ui
                if (character) {
                    L.HR();
                    E.Space(S.Spacing, false);

                    DrawCharacterState(character);
                }
            L.EV();
        L.ES();
    }

    /// draw the horizontal layout
    void DrawViewHorizontal(Character character) {
        L.BH(S.Margins);
            L.BV(G.MaxWidth(S.ColumnWidth));
                DrawStartWarpField();
                DrawCharacterSearch(character);
            L.EV();

            // show the state ui
            if (character) {
                E.Space(S.Spacing, false);

                m_ScrollPos = L.BS(m_ScrollPos);
                    L.BV(G.MaxWidth(S.ColumnWidth));
                        DrawCharacterState(character);
                    L.EV();
                L.ES();
            }
        L.EH();
    }

    // show selected start warp point
    void DrawStartWarpField() {
        // show current or selected character
        E.LabelField(
            "start warp",
            EditorStyles.boldLabel
        );

        m_StartWarp.InitialValue = E.TextField(
            "warp point",
            m_StartWarp.InitialValue
        );
    }

    /// draw the character input and child search
    void DrawCharacterSearch(Character character) {
        // show current or selected character
        E.LabelField(
            "current character",
            EditorStyles.boldLabel
        );

        m_Character = (Character)E.ObjectField(
            "character",
            character ?? m_Character,
            typeof(Character),
            allowSceneObjects: true
        );

        // show the character ui
        if (m_Character != null) {
            L.BV();
                // show query ui
                DrawChildSearch();

                // select the object
                G.Space(3f);
                if (G.Button("select it")) {
                    SelectChild();
                }

                // focus (press f) the object
                if (G.Button("focus it")) {
                    SelectChild();
                    SceneView.FrameLastActiveSceneView();
                }
            L.EV();
        }
    }

    /// show input field to query child obj of character
    void DrawChildSearch() {
        L.BH();
            m_Query = E.TextField(
                "child query",
                m_Query
            );

            if (m_Query != "") {
                G.Space(5f);

                if (G.Button("x", G.ExpandWidth(false))) {
                    ClearQuery();
                }
            }
        L.EH();
    }

    /// draw the character's current state
    void DrawCharacterState(Character character) {
        var state = new SerializedObject(character.Online)
            .FindProperty("m_RemoteState");

        E.PropertyField(state);
    }

    // -- commands --
    /// clear query
    void ClearQuery() {
        m_Query = "";
    }

    /// select the character or child
    void SelectChild() {
        if (m_Character == null) {
            return;
        }

        var selection = m_Character.transform;
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