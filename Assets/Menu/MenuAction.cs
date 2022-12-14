namespace Discone.Ui {

/// a globally-dispatchable menu action
public class MenuAction {
    /// show the dialog
    public sealed class ShowDialog: MenuAction {
        /// .
        public readonly MenuDialog Dialog;

        /// .
        public ShowDialog(MenuDialog dialog) {
            Dialog = dialog;
        }
    }
}

}