using UnityEngine;
using UnityEditor;
using UnityAtoms;

using E = UnityEditor.EditorGUILayout;
using G = UnityEngine.GUILayout;

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
    /// the current scroll position
    Vector2 m_ScrollPos;

    /// the search query
    string m_Query = "";

    /// the manually selected character
    DisconeCharacter m_Character;

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
        L.BH(new GUIStyle() {
            margin = new RectOffset(10, 10, 10, 10)
        });
            L.BV(G.MaxWidth(300.0f));
                // show current or selected character
                E.LabelField(
                    "current character",
                    EditorStyles.boldLabel
                );

                m_Character = (DisconeCharacter)E.ObjectField(
                    character ?? m_Character,
                    typeof(DisconeCharacter),
                    allowSceneObjects: true
                );

                // show the character ui
                if (m_Character != null) {
                    L.BV();
                        // show query ui
                        G.Space(5f);
                        DrawChildSearch();
                        G.Space(7f);

                        // select the object
                        if (G.Button("select it", G.ExpandWidth(false))) {
                            SelectChild();
                        }
                    L.EV();
                }
            L.EV();

            // show the state ui
            if (m_Character != null) {
                E.Space(15f, false);

                m_ScrollPos = L.BS(m_ScrollPos);
                    L.BV(G.ExpandWidth(true));
                        var state = new SerializedObject(m_Character)
                            .FindProperty("m_RemoteState");

                        E.PropertyField(state);
                    L.EV();
                L.ES();
            }
        L.EH();
    }

    /// show input field to query child obj of character
    void DrawChildSearch() {
        G.Label("child query");

        L.BH();
            m_Query = G.TextField(m_Query);

            if (m_Query != "") {
                G.Space(5f);

                if (G.Button("x", G.ExpandWidth(false))) {
                    ClearQuery();
                }
            }
        L.EH();
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