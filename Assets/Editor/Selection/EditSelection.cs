using UnityEngine;
using UnityEditor;

using E = UnityEditor.EditorGUILayout;
using G = UnityEngine.GUILayout;

namespace Discone.Editor {

/// a base class for selection editors
public sealed class EditSelection: EditorWindow {
    // -- props --
    /// the editor title
    const string k_Title = "edit selection";

    /// the editor namte
    const string k_Name = "edit";

    // -- props --
    Component[] m_Components;

    // -- lifecycle --
    /// show the window
    [MenuItem("GameObject/discone/edit selection")]
    static void Init() {
        var window = GetWindow<EditSelection>(
            title: k_Title,
            focus: true
        );

        window.Show();
    }

    void Awake() {
        m_Components = new Component[] {
            new RenameSelection(),
            new JitterSelection(),
            new ReplaceSelection(),
            new RescaleSelection(),
        };
    }

    void OnGUI() {
        L.BV();
            var n = m_Components.Length;
            for (var i = 0; i < n; i++) {
                var component = m_Components[i];

                // show title
                E.LabelField(component.Title, EditorStyles.boldLabel);

                // render component
                component.OnGUI();

                // add divider
                if (i < n - 1) {
                    E.Space(15.0f);
                }
            }
        L.EV();
    }

    // -- children --
    /// an edit selection component
    public abstract class Component {
        // -- lifecycle --
        /// the component title
        public abstract string Title { get; }

        /// render the component gui
        public abstract void OnGUI();

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
            Undo.SetCurrentGroupName(k_Name);
        }

        /// record the state of the objects in the current record
        protected void StoreUndoState(GameObject[] objs) {
            foreach (var obj in objs) {
                Undo.RegisterCompleteObjectUndo(obj, k_Name);
            }
        }

        /// record the creation of the object in the current record
        protected void StoreUndoCreate(GameObject obj) {
            Undo.RegisterCreatedObjectUndo(obj, k_Name);
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
    }
}

}