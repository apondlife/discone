using UnityEngine;
using UnityEditor;
using System.Linq;

using E = UnityEditor.EditorGUILayout;
using G = UnityEngine.GUILayout;

namespace Discone.Editor {

/// rename children of a game object; found on the internet
public sealed class RenameSelection: EditSelection.Component {
    // -- props --
    /// the name of the objects
    string m_Name;

    /// the start index
    int m_Start;

    // -- lifecycle --
    public override string Title {
        get => "rename";
    }

    public override void OnGUI() {
        // show fields
        m_Name = E.TextField("name", m_Name);
        m_Start = E.IntField("start index", m_Start);

        // show button
        if (G.Button("apply")) {
            Call();
        }
    }

    // -- commands --
    /// rename selected objects
    void Call() {
        var all = FindAll();

        // create undo record
        CreateUndoRecord(all);

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

}