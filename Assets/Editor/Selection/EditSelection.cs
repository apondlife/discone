using UnityEngine;
using UnityEditor;

using E = UnityEditor.EditorGUILayout;
using G = UnityEngine.GUILayout;

namespace Discone.Editor {

/// an editor for a selection of game objects
public sealed class EditSelection: EditorWindow {
    // -- props --
    /// the editor title
    const string k_Title = "edit selection";

    /// the editor namte
    const string k_Name = "edit";

    // -- props --
    /// the list of components
    Component[] m_Components;

    /// the scroll position
    Vector2 m_ScrollPos;

    // -- lifecycle --
    /// show the window
    [MenuItem("Window/discone/edit selection %#c")]
    static void Init() {
        var window = GetWindow<EditSelection>(
            title: k_Title,
            focus: true
        );

        window.Show();
    }

    void OnGUI() {
        m_ScrollPos = L.BS(m_ScrollPos);
            L.BV(
                S.Margins,
                G.MaxWidth(S.ColumnWidth)
            );
                var n = Components.Length;
                for (var i = 0; i < n; i++) {
                    var component = Components[i];

                    // show title
                    E.LabelField(
                        component.Title,
                        EditorStyles.boldLabel
                    );

                    // render component
                    component.OnGUI();

                    // add divider
                    if (i < n - 1) {
                        E.Space(15f);
                    }
                }
            L.EV();
        L.ES();
    }

    // -- queries --
    /// the list of components
    Component[] Components {
        get {
            if (m_Components == null) {
                m_Components = new Component[] {
                    new RenameSelection(),
                    new JitterSelection(),
                    new ReplaceSelection(),
                    new NormalizeSelection(),
                    new RealignSelection(),
                };
            }

            return m_Components;
        }
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
        protected void CreateUndoRecord(Object[] objs) {
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
        protected void StoreUndoState(Object[] objs) {
            foreach (var obj in objs) {
                Undo.RegisterCompleteObjectUndo(obj, k_Name);
            }
        }

        /// record the creation of the object in the current record
        protected void StoreUndoCreate(Object obj) {
            Undo.RegisterCreatedObjectUndo(obj, k_Name);
        }

        /// finish the current undo record
        protected void FinishUndoRecord() {
            Undo.IncrementCurrentGroup();
        }

        // -- queries --
        /// all selected objects
        protected Object[] FindAll() {
            return Selection.objects;
        }
    }
}

}