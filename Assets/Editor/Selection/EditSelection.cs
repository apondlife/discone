using UnityEngine;
using UnityEditor;

namespace Discone.Editor {

/// a base class for selection editors
public abstract class EditSelection: EditorWindow {
    // -- props --
    /// the editor name
    string m_Name;

    /// -- lifecycle --
    /// show the window
    protected static void ShowWindow<E>() where E: EditSelection {
        var window = GetWindow<E>(
            title: NameOf(typeof(E)),
            focus: true
        );

        window.Show();
    }

    // -- commands --
    /// create an undo record for the objects
    protected void CreateUndoRecord(GameObject[] objs) {
        StartUndoRecord();
        StoreUndoState(objs);
        FinishUndoRecord();
    }

    /// start an undo record
    protected void StartUndoRecord() {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName(Name);
    }

    /// record the state of the objects in the current record
    protected void StoreUndoState(GameObject[] objs) {
        foreach (var obj in objs) {
            Undo.RegisterCompleteObjectUndo(obj, Name);
        }
    }

    /// record the creation of the object in the current record
    protected void StoreUndoCreate(GameObject obj) {
        Undo.RegisterCreatedObjectUndo(obj, Name);
    }

    /// finish the current undo record
    protected void FinishUndoRecord() {
        Undo.IncrementCurrentGroup();
    }

    // -- queries --
    /// all selected objects
    protected GameObject[] FindAll() {
        return Selection.gameObjects;
    }

    /// the inferred name of this editor
    protected string Name {
        get {
            if (m_Name == null) {
                m_Name = NameOf(GetType());
            }

            return m_Name;
        }
    }

    /// the inferred name of an editor by type
    protected static string NameOf(System.Type type) {
        var name = type.Name;

        // remove selection
        var i = name.IndexOf("Selection");
        if (i >= 0) {
            name = name.Remove(i, name.Length - i);
        }

        // lowercase the name
        name = name.ToLower();

        return name;
    }
}

}