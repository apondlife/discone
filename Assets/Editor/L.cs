using UnityEngine;
using UnityEditor;

namespace Discone.Editor {

static class L {
    // -- queries --
    /// begin a vertical section
    public static Rect BH(GUIStyle style, params GUILayoutOption[] options) {
        return EditorGUILayout.BeginHorizontal(style, options);
    }

    /// begin a horizontal section
    public static Rect BH(params GUILayoutOption[] options) {
        return EditorGUILayout.BeginHorizontal(options);
    }

    /// end a horizontal section
    public static void EH() {
        EditorGUILayout.EndHorizontal();
    }

    /// begin a vertical section
    public static Rect BV(GUIStyle style, params GUILayoutOption[] options) {
        return EditorGUILayout.BeginVertical(style, options);
    }

    /// begin a vertical section
    public static Rect BV(params GUILayoutOption[] options) {
        return EditorGUILayout.BeginVertical(options);
    }

    /// end a vertical section
    public static void EV() {
        EditorGUILayout.EndVertical();
    }

    /// begin a scroll view
    public static Vector2 BS(Vector2 pos) {
        return EditorGUILayout.BeginScrollView(pos);
    }

    /// end a scroll view
    public static void ES() {
        EditorGUILayout.EndScrollView();
    }
}

}