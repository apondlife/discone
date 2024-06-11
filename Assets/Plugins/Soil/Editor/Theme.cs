using UnityEngine;
using EU = UnityEditor.EditorGUIUtility;

namespace Soil.Editor {

static class Theme {
    // -- constants --
    /// a gap between elements (2px); the spacing between the prefix label and field for unity built-ins
    public const float Gap1 = 2f;

    /// a gap between elements (4px); the spacing between fields for the Vector3 editor
    public const float Gap2 = 4f;

    /// a gap between elements (6px)
    public const float Gap3 = 6f;

    /// the width of each indent level
    public const float Gap_Indent = 16f;

    /// the editor background color
    public static readonly Color32 Bg = EU.isProSkin ? new Color32(56, 56, 56, 255) : new Color32(194, 194, 194, 255);
}

}