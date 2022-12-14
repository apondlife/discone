using System;

namespace Discone.Ui {

/// a menu dialog model; dispatched via MenuAction.ShowDialog
public record MenuDialog {
    /// .
    public readonly string Message;

    /// .
    public readonly Action OnConfirm;

    /// .
    public MenuDialog(string message, Action onConfirm) {
        Message = message;
        OnConfirm = onConfirm;
    }
}

}