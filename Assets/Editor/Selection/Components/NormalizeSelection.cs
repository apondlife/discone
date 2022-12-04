using UnityEngine;
using UnityEditor;

using E = UnityEditor.EditorGUILayout;
using G = UnityEngine.GUILayout;

namespace Discone.Editor {

/// normalize rotation of a single object, rotating and repositioning children
public sealed class NormalizeSelection: EditSelection.Component {
    // -- types --
    /// a stored transform record
    struct TransformRec {
        /// the stored world position
        public Vector3 Pos;

        /// the stored world scale
        public Vector3 Scl;

        /// the stored world rotation
        public Quaternion Rot;
    }

    // -- props --
    /// if the position should be normalized
    bool m_UpdatesPos = true;

    /// if the scale should be normalized
    bool m_UpdatesScl = true;

    /// if the rotation should be normalized
    bool m_UpdatesRot = true;

    // -- lifecycle --
    public override string Title {
        get => "normalize";
    }

    public override void OnGUI() {
        // show description
        E.LabelField(
            "sets the selection's pos/scale/rot to identity values and updates the transforms of the children",
            EditorStyles.wordWrappedLabel
        );

        L.BH();
            m_UpdatesPos = E.ToggleLeft(
                "postion",
                m_UpdatesPos,
                G.MaxWidth(69f)
            );

            m_UpdatesScl = E.ToggleLeft(
                "scale",
                m_UpdatesScl,
                G.MaxWidth(59f)
            );

            m_UpdatesRot = E.ToggleLeft(
                "rotation",
                m_UpdatesRot,
                G.MaxWidth(69f)
            );
        L.EH();

        // show button
        E.Space(5f);
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
            var p = t.localPosition;
            var s = t.localScale;
            var r = t.localRotation;

            // restore children from a list of records
            var records = new TransformRec[t.childCount];

            // capture transform record for each child
            var i = 0;
            foreach (Transform child in t) {
                // scale position & scale by parent
                var pos = child.position;
                var scl = child.localScale;

                if (m_UpdatesScl) {
                   scl.Scale(s);
                }

                // treat approximately-one scales as one, since normalizing
                // easily produces near-one scales do to precision loss
                if (Mathf.Approximately(Vector3.Dot(scl, Vector3.one), 3f)) {
                    scl = Vector3.one;
                }

                // save the record
                records[i] = new TransformRec() {
                    Pos = pos,
                    Scl = scl,
                    Rot = child.rotation,
                };

                i++;
            }


            // normalize the parent's scale
            if (m_UpdatesScl) {
                t.localScale = Vector3.one;
            }

            // normalize the parent's rotation
            if (m_UpdatesRot) {
                t.rotation = Quaternion.identity;
            }

            // normalize the parent's position
            if (m_UpdatesPos) {
                t.localPosition = Vector3.zero;
            }

            // restore children from records
            var j = 0;
            foreach (Transform child in t) {
                var record = records[j];
                if (m_UpdatesRot) {
                    child.rotation = record.Rot;
                }

                if (m_UpdatesScl) {
                    child.localScale = record.Scl;
                }

                child.position = record.Pos;

                j++;
            }
        }
    }
}

}