using UnityEditor;
using UnityEngine;

using E = UnityEditor.EditorGUI;
using U = UnityEditor.EditorGUIUtility;

namespace Soil.Editor {

[CustomPropertyDrawer(typeof(DynamicEase))]
public class DynamicEaseDrawer: PropertyDrawer {
    // -- props --
    readonly float[] m_Values = DynamicEaseConfigDrawer.CreateBuffer();

    // -- PropertyDrawer --
    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
        var height = DynamicEaseConfigDrawer.GetPropertyHeight(prop);
        if (prop.isExpanded) {
            height += U.standardVerticalSpacing + U.singleLineHeight;
            height += U.standardVerticalSpacing + U.singleLineHeight;
        }

        return height;
    }

    public override void OnGUI(Rect r, SerializedProperty prop, GUIContent label) {
        // unclear why this is sometimes called with a rect of width 1f
        if (r.width <= 1f) {
            return;
        }

        E.BeginProperty(r, label, prop);

        // get the rect for a line
        var rl = r;
        rl.height = U.singleLineHeight;

        // get attrs
        var pConfig = prop.FindPropertyRelative("m_Config");
        var pConfigSource = prop.FindPropertyRelative("m_ConfigSource");
        var pIsDisabled = prop.FindPropertyRelative("m_IsDisabled");

        // draw label w/ indent
        r = rl;
        prop.isExpanded = E.Foldout(r, prop.isExpanded, new GUIContent(label));

        // move rect past the label
        var lw = U.labelWidth + Theme.Gap1;
        r.x += lw;
        r.width -= lw;

        // disable fields if external
        var configSource = (DynamicEase.ConfigSource)pConfigSource.intValue;
        E.BeginDisabledGroup(configSource == DynamicEase.ConfigSource.External);

        // draw fzr fields
        var config = DynamicEaseConfigDrawer.DrawFzr(r, pConfig);

        E.EndDisabledGroup();

        // draw graph on foldout
        if (prop.isExpanded) {
            E.indentLevel += 1;

            // move to beginning of line
            r.y = r.yMax + U.standardVerticalSpacing;
            r.x = rl.x;
            r.width = rl.width;

            // draw config source
            r.height = rl.height;
            E.PropertyField(r, pConfigSource, new GUIContent("Config"));

            // move to beginning of line
            r.y = r.yMax + U.standardVerticalSpacing;
            r.x = rl.x;
            r.width = rl.width;

            // draw toggle
            r.height = rl.height;
            E.PropertyField(r, pIsDisabled);

            // move to beginning of line
            r.y = r.yMax + Theme.Gap3;
            r.x = rl.x;
            r.width = rl.width;

            // draw graph
            DynamicEaseConfigDrawer.DrawGraph(r, config, m_Values);

            E.indentLevel -= 1;
        }

        E.EndProperty();
    }
}

}