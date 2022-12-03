using UnityEngine;
using UnityEditor;

using E = UnityEditor.EditorGUILayout;
using G = UnityEngine.GUILayout;

namespace Discone.Editor {

/// normalize scale of a single object, rescaling children
public sealed class RescaleSelection: EditSelection.Component {
    // -- lifecycle --
    public override string Title {
        get => "rescale";
    }

    public override void OnGUI() {
        // show description
        E.LabelField(
            "sets scale of to <1,1,1> and adjusts the scale of all children",
            EditorStyles.wordWrappedLabel
        );

        // show button
        if (G.Button("apply")) {
            Call();
        }
    }

    // -- commands --
    /// rescale the selected objects
    public void Call() {
        var all = FindAll();

        // create undo record
        CreateUndoRecord(all);

        // for each object
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

}