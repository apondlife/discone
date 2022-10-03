using UnityEngine;
using UnityEditor;
using System.Linq;

/// rename children of a game object; found on the internet
public sealed class RenameSelected: EditorWindow {
    // -- constants --
    /// the operation name
    const string k_Name = "rename";

    // -- props --
    /// the name of the objects
    string m_Name;

    /// the start index
    int m_Start;

    /// -- lifecycle --
    /// show the window
    [MenuItem("GameObject/Selection/rename")]
    public static void Init() {
        EditorWindow window = GetWindow<RenameSelected>();
        window.name = k_Name;
        window.Show();
    }

    void OnGUI() {
        // show fields
        m_Name = EditorGUILayout.TextField("name", m_Name);
        m_Start = EditorGUILayout.IntField("start index", m_Start);

        // show button
        if (GUILayout.Button(k_Name)) {
            Rename();
        }
    }

    // -- commands --
    /// rename selected objects
    void Rename() {
        var all = Selection.gameObjects;

        // create undo record
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName(k_Name);

        foreach (var obj in all) {
            Undo.RegisterCompleteObjectUndo(obj, k_Name);
        }

        Undo.IncrementCurrentGroup();

        // sort selected objects by index
        var sorted = all
            .OrderBy((o) => o.transform.GetSiblingIndex());

        // rename all the objects
        var i = 0;
        foreach (var obj in sorted) {
            obj.name = $"{m_Name}{m_Start + i}";
            i++;
        }
    }
}