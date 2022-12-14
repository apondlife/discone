using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Discone.Ui {

/// the dialog pseudo-page
sealed class DialogPage: UIBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip(".")]
    [SerializeField] TMP_Text m_Message;

    [Tooltip(".")]
    [SerializeField] Button m_CancelButton;

    [Tooltip(".")]
    [SerializeField] Button m_ConfirmButton;

    // -- props --
    /// the current dialog, if any
    MenuDialog m_Dialog;

    /// a callback when the dialog finishes, regardless of selection
    Action m_OnComplete;

    /// .
    DisposeBag m_Subscriptions = new DisposeBag();

    // -- lifecycle --
    protected override void Start() {
        base.Start();

        // bind events
        m_Subscriptions
            .Add(m_CancelButton.onClick, OnCancelPressed)
            .Add(m_ConfirmButton.onClick, OnConfirmPressed);
    }

    protected override void OnDestroy() {
        base.OnDestroy();

        // unbind events
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// show the page with the dialog and callabck
    public void Show(MenuDialog dialog, Action onComplete) {
        m_Dialog = dialog;
        m_OnComplete = onComplete;

        // render the dialog
        m_Message.text = dialog.Message;
    }

    /// finalize the dialog
    void Complete() {
        m_OnComplete();

        m_Dialog = null;
        m_OnComplete = null;
    }

    // -- events --
    /// .
    void OnCancelPressed() {
        Complete();
    }

    /// .
    void OnConfirmPressed() {
        m_Dialog.OnConfirm();
        Complete();
    }
}

}