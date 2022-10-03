using UnityEngine;
using UnityEditor;

/// normalize scale of a single object, rescaling children
public sealed class RescaleSelected: EditorWindow {
    // -- constants --
    /// the operation name
    const string k_Name = "rescale";

    // -- lifecycle --
    /// normalize the scale
    [MenuItem("GameObject/Selection/rescale")]
    public static void Call() {
        var all = Selection.gameObjects;

        // create undo record
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName(k_Name);

        foreach (var obj in all) {
            Undo.RegisterFullObjectHierarchyUndo(obj, k_Name);
        }

        Undo.IncrementCurrentGroup();

        // for each selected object
        foreach (var obj in all) {
            var t = obj.transform;
            var s = t.localScale;

            // rescale its children
            foreach (Transform child in t) {
                var scl = child.localScale;
                var pos = child.localPosition;

                pos.Scale(s);
                scl.Scale(s);

                child.localScale = scl;
                child.localPosition = pos;
            }

            // and normalize its scale
            t.localScale = Vector3.one;
        }
    }
}