using NaughtyAttributes;
using Yarn.Markup;
using Yarn.Unity;

namespace Discone.Ui {

partial class MechanicDialogueView {
    [Button("Show Test Line")]
    void ShowTestLine() {
        var line = new LocalizedLine();
        var text = new MarkupParseResult();
        text.Text = "";

        var n = UnityEngine.Random.Range(2, 10);
        for (var i = 0; i < n; i++) {
            text.Text += "asdf ";
        }

        line.Text = text;
        RunLine(line, null);
    }
}

}