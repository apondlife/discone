using UnityEngine;
using UnityEditor;
using System.Linq;

/// rename children of a game object; found on the internet
public sealed class RenameSelected: EditorWindow {
    // -- constants --
    /// the window size
    static readonly Vector2Int k_Size = new Vector2Int(250, 100);

    // -- props --
    /// the name of the objects
    string m_Name;

    /// the start index
    int m_Start;

    /// -- lifecycle --
    void OnGUI() {
        m_Name = EditorGUILayout.TextField("name", m_Name);
        m_Start = EditorGUILayout.IntField("start index", m_Start);

        if (GUILayout.Button("rename")) {
            Rename();
        }
    }

    // -- commands --
    /// rename selected objects
    void Rename() {
        // sort selected objects by index
        var sorted = Selection
            .gameObjects
            .OrderBy((o) => o.transform.GetSiblingIndex());

        // rename all the objects
        var i = 0;
        foreach (var obj in sorted) {
            obj.name = $"{m_Name}{m_Start + i}";
            i++;
        }
    }

    // -- factories --
    /// show the window
    [MenuItem("GameObject/Selection/rename")]
    public static void Create() {
        EditorWindow window = GetWindow<RenameSelected>();
        window.name = "rename selection";
        window.minSize = k_Size;
        window.maxSize = k_Size;
    }
}