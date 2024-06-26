using System;
using UnityEditor;
using UnityEngine;

using E = UnityEditor.EditorGUI;
using U = UnityEditor.EditorGUIUtility;

namespace Soil.Editor {

[CustomPropertyDrawer(typeof(DynamicEase.Config))]
public class DynamicEaseConfigDrawer: PropertyDrawer {
    // -- constants --
    /// the duration for the ease graph
    const float k_Duration = 2f;

    /// the count for the ease graph
    const int k_Count = 120;

    /// the duration for the ease graph
    const float k_Delta = k_Duration / k_Count;

    /// the height of the graph
    const float k_GraphHeight = 60f;

    /// the drawing material
    static readonly Material s_Material;

    // -- setup --
    static DynamicEaseConfigDrawer() {
        var material = new Material(Shader.Find("UI/Default"));
        material.hideFlags = HideFlags.HideAndDontSave;
        material.shader.hideFlags = HideFlags.HideAndDontSave;
        s_Material = material;
    }

    // -- props --
    /// the list of graph values
    readonly float[] m_Values = CreateBuffer();

    // -- PropertyDrawer --
    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
        return GetPropertyHeight(prop);
    }

    public static float GetPropertyHeight(SerializedProperty prop) {
        var height = U.singleLineHeight;
        if (prop.isExpanded) {
            height += Theme.Gap3 + k_GraphHeight;
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

        // draw label w/ indent
        r = rl;
        prop.isExpanded = E.Foldout(r, prop.isExpanded, new GUIContent(label));

        // move rect past the label
        var lw = U.labelWidth + Theme.Gap1;
        r.x += lw;
        r.width -= lw;

        // draw fzr fields
        var newConfig = DrawFzr(r, prop);

        // draw graph on foldout
        if (prop.isExpanded) {
            E.indentLevel += 1;

            // move to beginning of line
            r.x = rl.x;
            r.y = r.yMax + Theme.Gap3;
            r.width = rl.width;

            // draw graph
            DrawGraph(r, newConfig, m_Values);

            E.indentLevel -= 1;
        }

        E.EndProperty();
    }

    // -- queries --
    /// create a buffer for graph values
    public static float[] CreateBuffer() {
        return new float[k_Count];
    }

    // -- elements --
    /// draw the fzr fields
    public static DynamicEase.Config DrawFzr(Rect r, SerializedProperty prop) {
        // get attrs
        var pF = prop.FindProp(nameof(DynamicEase.Config.F));
        var pZ = prop.FindProp(nameof(DynamicEase.Config.Z));
        var pR = prop.FindProp(nameof(DynamicEase.Config.R));

        // draw fzr fields
        var labels = new GUIContent[] { new(pF.name), new(pZ.name), new(pR.name) };
        var values = new[] { pF.floatValue, pZ.floatValue, pR.floatValue };
        E.MultiFloatField(r, labels, values);

        // recalculate config
        var config = new DynamicEase.Config(f: values[0], z: values[1], r: values[2]);

        // update attrs
        pF.floatValue = config.F;
        pZ.floatValue = config.Z;
        pR.floatValue = config.R;

        prop.SetValue(nameof(DynamicEase.Config.K1), config.K1);
        prop.SetValue(nameof(DynamicEase.Config.K2), config.K2);
        prop.SetValue(nameof(DynamicEase.Config.K3), config.K3);

        return config;
    }

    /// draw the graph visualizing the dynamic ease
    public static void DrawGraph(Rect r, DynamicEase.Config config, float[] values) {
        var indent = Theme.Gap_Indent * E.indentLevel;
        r.x += indent;
        r.width -= indent;
        r.height = k_GraphHeight;

        // work with a copy of the ease to avoid changing game state
        var ease = new DynamicEase(config);

        // keep track of extents of the graph
        var range = new FloatRange(
            min: float.MaxValue,
            max: float.MinValue
        );

        // sample values from the ease
        ease.Init(Vector3.zero);
        for (var i = 0; i < k_Count; i++) {
            ease.Update(k_Delta, Vector3.up);

            var value = ease.Value.y;
            if (value < range.Min) {
                range.Min = value;
            }

            if (value > range.Max) {
                range.Max = value;
            }

            values[i] = ease.Value.y;
        }

        // start drawing into a render texture
        s_Material.SetPass(0);
        var texture = RenderTexture.GetTemporary((int)r.width, (int)r.height);
        RenderTexture.active = texture;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0f, texture.width, 0f, texture.height);

        // the rect boundaries
        var x0 = 0f;
        var x1 = texture.width;

        // the y-position
        var y = 0f;

        // the operation to filter subpixel values
        var filter = (Func<float, float>)Mathf.Floor;

        // draw the background
        GL.Clear(false, true, Theme.Bg);

        // draw the boundaries
        GL.Begin(GL.LINE_STRIP);
        GL.Color(Color.LightGray);

        y = filter(range.InverseLerp(0f) * texture.height);
        GL.Vertex(new Vector3(x0, y, 0f));
        GL.Vertex(new Vector3(x1, y, 0f));

        GL.End();

        // draw the 1-line
        GL.Begin(GL.LINE_STRIP);
        GL.Color(Color.LightGreen);

        y = filter(range.InverseLerp(1f) * texture.height);
        GL.Vertex(new Vector3(x0, y, 0f));
        GL.Vertex(new Vector3(x1, y, 0f));

        GL.End();

        // draw the ease
        GL.Begin(GL.LINE_STRIP);
        GL.Color(Color.Cyan);

        for (var i = 0; i < k_Count; i++) {
            var value = values[i];
            GL.Vertex(new Vector3(
                x: filter(i * k_Delta * texture.width),
                y: filter(range.InverseLerp(value) * texture.height),
                z: 0f
            ));
        }

        GL.End();

        // clean up drawing context & render texture
        GL.PopMatrix();
        RenderTexture.active = null;
        E.DrawPreviewTexture(r, texture);
        RenderTexture.ReleaseTemporary(texture);
    }
}

}