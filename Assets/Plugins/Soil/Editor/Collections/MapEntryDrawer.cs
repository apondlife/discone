using UnityEditor;
using UnityEngine;
using G = UnityEditor.EditorGUI;

namespace Soil.Editor {

/// a drawer for a map entry
[CustomPropertyDrawer(typeof(Map<,>.Entry))]
public class MapEntryDrawer: PropertyDrawer {
    // -- PropertyDrawer --
    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
        var val = prop.FindPropertyRelative("Val");
        G.PropertyField(pos, val, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
        var val = prop.FindPropertyRelative("Val");
        return G.GetPropertyHeight(val, true);
    }
}

}