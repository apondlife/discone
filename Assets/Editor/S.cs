using UnityEngine;
using UnityEditor;

namespace Discone.Editor {

static class S {
    // -- props --
    /// the margin
    public const int Margin = 10;

    /// the spacing
    public const float Spacing = 15f;

    /// the column max width
    public const float ColumnWidth = 300f;

    // -- statics --
    /// the margin styles
    public static GUIStyle Margins;

    /// the horizontal line style
    public static GUIStyle Rule;

    // -- commands --
    /// initialize the styles
    public static void Init() {
        if (Margins != null) {
            return;
        }

        // init margins
        Margins = new GUIStyle() {
            margin = new RectOffset(Margin, Margin, Margin, Margin)
        };

        // init rule
        var rule = new GUIStyle();
        rule.normal.background = EditorGUIUtility.whiteTexture;
        rule.margin = new RectOffset(Margin, Margin, (int)Spacing, (int)Spacing);
        rule.fixedHeight = 1f;
        Rule = rule;
    }
}

}