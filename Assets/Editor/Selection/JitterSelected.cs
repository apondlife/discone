using UnityEngine;
using UnityEditor;
using System.Linq;

/// randomly rotate the selected objects by a bounded amount
public sealed class JitterSelected: EditorWindow {
    // -- constants --
    /// the operation name
    const string k_Name = "jitter";

    // -- props --
    /// the max rotation for each object
    Vector3 m_MaxRotation;

    /// the max translation for each object
    Vector3 m_MaxTranslation;

    /// -- lifecycle --
    /// show the window
    [MenuItem("GameObject/Selection/jitter")]
    public static void Init() {
        EditorWindow window = GetWindow<JitterSelected>();
        window.name = k_Name;
        window.Show();
    }

    void OnGUI() {
        // show rotation
        m_MaxRotation = EditorGUILayout.Vector3Field("max rotation", m_MaxRotation);
        if (GUILayout.Button("rotate")) {
            Rotate();
        }

        // show translation
        m_MaxTranslation = EditorGUILayout.Vector3Field("max translation", m_MaxTranslation);
        if (GUILayout.Button("translate")) {
            Translate();
        }
    }

    // -- commands --
    /// rotate selected objects
    void Rotate() {
        var all = Selection.gameObjects;

        // create undo record
        CreateUndoRecord(all);

        // jitter the rotation of all the objects
        foreach (var obj in all) {
            var t = obj.transform;
            var e = t.localEulerAngles + Sample(m_MaxRotation);
            t.localEulerAngles = e;
        }
    }

    /// translate selected objects
    void Translate() {
        var all = Selection.gameObjects;

        // create undo record
        CreateUndoRecord(all);

        // jitter the position of all the objects
        foreach (var obj in all) {
            var t = obj.transform;
            var p = t.position + Sample(m_MaxTranslation);
            t.position = p;
        }
    }

    /// create an undo record for the objects
    void CreateUndoRecord(GameObject[] objs) {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName(k_Name);

        foreach (var obj in objs) {
            Undo.RegisterCompleteObjectUndo(obj, k_Name);
        }

        Undo.IncrementCurrentGroup();
    }

    // -- queries --
    /// sample a random vector given a max vector
    Vector3 Sample(Vector3 max) {
        var res = Vector3.zero;
        res.x += max.x * Random.Range(-1.0f, 1.0f);
        res.y += max.y * Random.Range(-1.0f, 1.0f);
        res.z += max.z * Random.Range(-1.0f, 1.0f);
        return res;
    }
}