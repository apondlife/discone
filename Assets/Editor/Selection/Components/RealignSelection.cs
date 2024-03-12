using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

using E = UnityEditor.EditorGUILayout;
using G = UnityEngine.GUILayout;

namespace Discone.Editor {

/// realign objects on an axis
public sealed class RealignSelection: EditSelection.Component {
    // -- types --
    [Flags]
    enum Axis {
        N = 0,
        X = 1 << 1,
        Y = 1 << 2,
        Z = 1 << 3,
        A = X | Y | Z,
    }

    // -- props --
    /// the selected acis
    Axis m_Axis;

    // -- EditorSelection.Component --
    public override string Title {
        get => "realign";
    }

    public override void OnGUI() {
        // show description
        E.LabelField(
            "align the axis of the selected objects on along the axis of the first object",
            EditorStyles.wordWrappedLabel
        );

        // show axis fields
        var next = ~Axis.N;
        L.BH();
            next &= DrawToggle("x", Axis.X);
            next &= DrawToggle("y", Axis.Y);
            next &= DrawToggle("z", Axis.Z);
        L.EH();

        // update fields
        m_Axis = next & Axis.A;

        // show button
        G.Space(3f);
        if (G.Button("apply")) {
            Call();
        }
    }

    // -- ui --
    /// draw a toggle and return the axis if it was toggled on
    Axis DrawToggle(string label, Axis axis) {
        var prev = m_Axis == axis;
        var next = E.ToggleLeft(
            label,
            prev,
            G.MaxWidth(29f)
        );

        // if off, turn yourself off
        if (!next) {
            return ~axis;
        }

        // if on, and was on, don't change anything
        if (prev == next) {
            return Axis.A;
        }

        // if this was a change, turn everything else off
        return axis;
    }

    // -- commands --
    /// rotate selected objects
    void Call() {
        // validate args
        if (m_Axis == Axis.N) {
            Log.Editor.I($"must have an axis to align on");
            return;
        }

        // find all objs
        var all = Selection.objects;

        // validate selection
        if (all.Length < 2) {
            Log.Editor.I($"must have at least two objects selected to align");
            return;
        }

        // create undo record
        CreateUndoRecord(all);

        // pick the source transform
        var src = (all[0] as GameObject).transform;

        // for each object except the first
        foreach (var obj in all.OfType<GameObject>()) {
            var dst = obj.transform;
            if (dst == src) {
                continue;
            }

            // update transform
            switch (m_Axis) {
            case Axis.X:
                break;
            case Axis.Y:
                dst.rotation = Quaternion.LookRotation(dst.forward, src.up); break;
            case Axis.Z:
                dst.rotation = Quaternion.LookRotation(src.forward, dst.up); break;
            default: break;
            }
        }

        // finish undo record
        FinishUndoRecord();
    }
}

}