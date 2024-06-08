using System.Reflection;
using UnityEditor;
using UnityEngine;
using E = UnityEditor.EditorGUI;
using U = UnityEditor.EditorGUIUtility;

namespace Soil.Editor {

[CustomPropertyDrawer(typeof(DynamicEase))]
public class DynamicEaseDrawer: PropertyDrawer {
    // -- constants --
    /// the duration for the ease chart
    const float k_Duration = 2f;

    /// the count for the ease chart
    const int k_Count = 120;

    /// the duration for the ease chart
    const float k_Delta = k_Duration / k_Count;

    /// the height of the chart
    const float k_ChartHeight = 18f;

    /// the drawing material
    static readonly Material s_Material;

    // -- setup --
    static DynamicEaseDrawer() {
        var material = new Material(Shader.Find("GUI/Text Shader"));
        material.hideFlags = HideFlags.HideAndDontSave;
        material.shader.hideFlags = HideFlags.HideAndDontSave;
        s_Material = material;
    }

    // -- props --
    /// the list of chart values
    float[] m_Values = new float[k_Count];

    // -- PropertyDrawer --
    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
        return U.singleLineHeight + U.standardVerticalSpacing + k_ChartHeight;
    }

    public override void OnGUI(Rect r, SerializedProperty prop, GUIContent label) {
        // get the rect for a line
        var rl = r;
        rl.height = U.singleLineHeight;

        // draw the full height of the property
        // r.height = GetPropertyHeight(prop, label);
        E.BeginProperty(r, label, prop);

        // get attrs
        var pF = prop.FindPropertyRelative("F");
        var pZ = prop.FindPropertyRelative("Z");
        var pR = prop.FindPropertyRelative("R");

        // draw label w/ indent
        r = rl;
        E.LabelField(r, label);

        // move rect past the label
        var lw = U.labelWidth + Theme.Gap1;
        r.x += lw;
        r.width -= lw;

        // draw fzr fields
        var labels = new GUIContent[] { new(pF.name), new(pZ.name), new(pR.name) };
        var values = new[] { pF.floatValue, pZ.floatValue, pR.floatValue };
        E.MultiFloatField(r, labels, values);

        pF.floatValue = values[0];
        pZ.floatValue = values[1];
        pR.floatValue = values[2];

        // draw next line
        r = rl;
        r.x += U.labelWidth;
        r.y += rl.height + U.standardVerticalSpacing;
        r.width -= U.labelWidth;
        r.height = k_ChartHeight;

        // get a copy of the dynamic ease
        var owner = prop.serializedObject.targetObject;
        var ownerType = owner.GetType();

        var easeField = ownerType.GetField(prop.name, BindingFlags.Instance | BindingFlags.NonPublic);
        var easeBox = easeField.GetValue(owner) as DynamicEase?;
        var ease = easeBox.Value.Clone();

        // calculate the values
        var range = new FloatRange(
            min: float.MaxValue,
            max: float.MinValue
        );

        ease.Init(Vector3.zero);
        for (var i = 0; i < k_Count; i++) {
            ease.Update(k_Delta, Vector3.up);

            var value = ease.Pos.y;
            if (value < range.Min) {
                range.Min = value;
            }

            if (value > range.Max) {
                range.Max = value;
            }

            m_Values[i] = ease.Pos.y;
        }

        // draw chart
        s_Material.SetPass(0);
        var texture = RenderTexture.GetTemporary((int)rl.width, (int)rl.height);
        RenderTexture.active = texture;

        GL.PushMatrix();
        GL.LoadPixelMatrix(0f, texture.width, 0f, texture.height);
        GL.Clear(false, true, Theme.Bg);

        // the rect boundaries
        var x0 = 0f;
        var x1 = texture.width;

        // draw the boundaries
        GL.Begin(GL.LINE_STRIP);
        GL.Color(Color.LightGray);

        var y = range.InverseLerp(0f) * texture.height;
        GL.Vertex(new Vector3(x0, y, 0f));
        GL.Vertex(new Vector3(x1, y, 0f));

        GL.End();

        // draw the 1-line
        GL.Begin(GL.LINE_STRIP);
        GL.Color(Color.Cyan);

        y = range.InverseLerp(1f) * texture.height;
        GL.Vertex(new Vector3(0f, y, 0f));
        GL.Vertex(new Vector3(1f * texture.width, y, 0f));

        GL.End();

        // draw the ease
        GL.Begin(GL.LINE_STRIP);
        GL.Color(Color.Yellow);

        for (var i = 0; i < k_Count; i++) {
            var value = m_Values[i];
            GL.Vertex(new Vector3(
                x: i * k_Delta * texture.width,
                y: range.InverseLerp(value) * texture.height,
                z: 0f
            ));
        }

        GL.End();

        GL.PopMatrix();

        RenderTexture.active = null;
        E.DrawPreviewTexture(r, texture);
        RenderTexture.ReleaseTemporary(texture);

        E.EndProperty();
    }
}

}