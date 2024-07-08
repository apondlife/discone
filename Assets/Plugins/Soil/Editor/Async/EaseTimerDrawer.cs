using UnityEngine;
using UnityEditor;

using E = UnityEditor.EditorGUI;
using U = UnityEditor.EditorGUIUtility;

namespace Soil.Editor {

// TODO: show the current time when the game is playing
[CustomPropertyDrawer(typeof(EaseTimer))]
sealed class EaseTimerDrawer: PropertyDrawer {
    // -- commands --
    public override void OnGUI(Rect r, SerializedProperty prop, GUIContent label) {
        E.BeginProperty(r, label, prop);

        // get attrs
        var config = prop.FindProp("Config", isPrivate: true);
        var configSource = prop.FindProp("ConfigSource", isPrivate: true);

        // draw label w/ indent
        E.LabelField(r, label);

        // reset indent so that it doesn't affect inline fields
        var indent = E.indentLevel;
        E.indentLevel = 0;

        // move rect past the label
        var lw = U.labelWidth + Theme.Gap1;
        r.x += lw;
        r.width -= lw;

        // draw the config source
        E.PropertyField(r, configSource, new GUIContent("Config"));

        // draw config fields (disabled if external)
        E.BeginDisabledGroup(configSource.intValue == (int)EaseTimer.ConfigSource.External);
        EaseTimerConfigDrawer.DrawInput(r, config);
        E.EndDisabledGroup();

        // reset indent level
        E.indentLevel = indent;

        E.EndProperty();
    }
}

}