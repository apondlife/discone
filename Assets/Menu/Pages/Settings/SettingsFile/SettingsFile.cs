using UnityAtoms;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Discone.Ui {

sealed class SettingsFile: UIBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("a label for the file")]
    [SerializeField] string m_Label;

    [Tooltip("the relative path to the file")]
    [SerializeField] string m_Path; // TODO: this could be an enum

    // -- dispatched --
    [Header("dispatched")]
    [Tooltip("dispatch a menu action")]
    [SerializeField] MenuActionEvent m_Action;

    // -- refs --
    [Header("refs")]
    [Tooltip("the record store")]
    [SerializeField] Store m_Store;

    [Tooltip(".")]
    [SerializeField] Button m_CopyButton;

    [Tooltip(".")]
    [SerializeField] Button m_DeleteButton;

    // -- props --
    /// .
    DisposeBag m_Subscriptions = new DisposeBag();

    // -- lifecycle --
    protected override void Start() {
        base.Start();

        // bind events
        m_Subscriptions
            .Add(m_CopyButton.onClick, OnCopyPressed)
            .Add(m_DeleteButton.onClick, OnDeletePressed);
    }

    protected override void OnDestroy() {
        base.OnDestroy();

        // unbind events
        m_Subscriptions.Dispose();
    }

    // -- events --
    /// .
    void OnCopyPressed() {
        m_Store.CopyPath(m_Path);
    }

    /// .
    void OnDeletePressed() {
        var dialog = new MenuDialog(
            $"are you sure you want to delete your {m_Label}?",
            onConfirm: () => {
                m_Store.Delete(m_Path);
            }
        );

        m_Action.Raise(new MenuAction.ShowDialog(dialog));
    }
}

}
