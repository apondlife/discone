using NaughtyAttributes;
using Yarn.Markup;
using Yarn.Unity;

namespace Discone {

sealed partial class Mechanic {
    #if UNITY_EDITOR
    [Button("Show Test Line")]
    void ShowTestLine() {
        var view = FindDialogueView();
        if (!view) {
            return;
        }

        var line = new LocalizedLine();
        var text = new MarkupParseResult();
        text.Text = "";

        var n = UnityEngine.Random.Range(2, 10);
        for (var i = 0; i < n; i++) {
            text.Text += "asdf ";
        }

        line.Text = text;
        view.RunLine(line, null);
    }
    #endif
}

}