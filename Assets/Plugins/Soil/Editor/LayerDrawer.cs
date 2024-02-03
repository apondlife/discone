using UnityEditor;
using UnityEngine;

using E = UnityEditor.EditorGUI;

namespace Soil.Editor {

[CustomPropertyDrawer(typeof(Layer))]
public class LayerDrawer: PropertyDrawer {
    // -- PropertyDrawer --
    public override void OnGUI(Rect r, SerializedProperty prop, GUIContent label) {
        E.BeginProperty(r, label, prop);

        var index = prop.FindPropertyRelative("m_Index");
        index.intValue = E.LayerField(r, label, index.intValue);

        E.EndProperty();
    }
}

}