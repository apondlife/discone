using UnityEngine;
using UnityEditor;

namespace Discone.Editor {

/// randomly rotate the selected objects by a bounded amount
public sealed class ReplaceSelection: EditSelection {
    // -- props --
    /// the prefab to replace with
    GameObject m_Prefab;

    /// -- lifecycle --
    [MenuItem("GameObject/Selection/replace")]
    public static void Init() {
        ShowWindow<ReplaceSelection>();
    }

    void OnGUI() {
        // show prefab field
        m_Prefab = (GameObject)EditorGUILayout.ObjectField(
            "prefab",
            m_Prefab,
            typeof(GameObject),
            false
        );

        // show button
        if (GUILayout.Button("replace")) {
            Call();
        }
    }

    // -- commands --
    /// rotate selected objects
    void Call() {
        // validate args
        if (m_Prefab == null) {
            Debug.Log($"[editor] must have an object to replace");
            return;
        }

        // find all objs
        var all = Selection.gameObjects;

        // create undo record
        StartUndoRecord();
        StoreUndoState(all);

        // get the prefab type
        var type = PrefabUtility.GetPrefabAssetType(m_Prefab);

        // for each object
        foreach (var obj in all) {
            GameObject sub;

            // create the substitute
            if (type == PrefabAssetType.NotAPrefab) {
                sub = Instantiate(m_Prefab);
            } else {
                sub = (GameObject)PrefabUtility.InstantiatePrefab(m_Prefab);
            }

            StoreUndoCreate(sub);

            // merge name
            sub.name = obj.name;

            // merge transform
            var to = obj.transform;
            var ts = sub.transform;
            ts.parent = to.parent;
            ts.localPosition = to.localPosition;
            ts.localRotation = to.localRotation;
            ts.localScale = to.localScale;
            ts.SetSiblingIndex(to.GetSiblingIndex());

            // destroy the old object
            Undo.DestroyObjectImmediate(obj);
        }

        // finish undo record
        FinishUndoRecord();
    }
}

}