using UnityEditor;
using UnityEngine;
using G = UnityEditor.EditorGUI;
using U = UnityEditor.EditorGUIUtility;

namespace Soil.Editor {

/// a drawer for a map
[CustomPropertyDrawer(typeof(Map<,>))]
public class MapDrawer: PropertyDrawer {
    // -- constants --
    /// the height of the warning box
    const float k_WarningHeight = 1.5f;

    // -- PropertyDrawer --
    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
        var list = prop.FindPropertyRelative("m_Entries");

        // draw list of entries
        G.PropertyField(pos, list, label, true);

        // draw warning, if any
        var warning = prop.FindPropertyRelative("m_Warning").stringValue;
        if (!string.IsNullOrEmpty(warning)) {
            pos.y += G.GetPropertyHeight(list, true);
            if (!list.isExpanded) {
                pos.y += U.singleLineHeight;
            }

            pos.height = k_WarningHeight * U.singleLineHeight;
            pos = G.IndentedRect(pos);

            G.HelpBox(pos, warning, MessageType.Warning);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        // height of of the list
        var list = property.FindPropertyRelative("m_Entries");
        var height = G.GetPropertyHeight(list, true);

        // and the warning, if any
        var warning = property.FindPropertyRelative("m_Warning").stringValue;
        if (!string.IsNullOrEmpty(warning)) {
            height += k_WarningHeight * U.singleLineHeight;
            if (!list.isExpanded) {
                height += U.singleLineHeight;
            }
        }

        return height;
    }
}

}