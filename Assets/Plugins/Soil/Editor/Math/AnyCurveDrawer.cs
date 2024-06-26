using System;
using UnityEngine;
using UnityEditor;

using E = UnityEditor.EditorGUI;
using U = UnityEditor.EditorGUIUtility;

namespace Soil.Editor {

[CustomPropertyDrawer(typeof(AnyCurve))]
public sealed class AnyCurveDrawer: PropertyDrawer {
    // -- constants --
    /// the width of the type input
    const float k_TypeInputWidth = 80f;

    // -- commands --
    public override void OnGUI(Rect r, SerializedProperty prop, GUIContent label) {
        E.BeginProperty(r, label, prop);

        // get attrs
        var type = prop.FindProp(nameof(AnyCurve.Type));
        var serialized = prop.FindProp(nameof(AnyCurve.Serialized), isPrivate: true);
        var src = serialized.FindProp(nameof(AnyCurve.Serialized.Src));
        var dst = serialized.FindProp(nameof(AnyCurve.Serialized.Dst));
        var curve = serialized.FindProp(nameof(AnyCurve.Serialized.Curve));

        // draw label w/ indent
        E.LabelField(r, label);

        // reset indent so that it doesn't affect inline fields
        var indent = E.indentLevel;
        E.indentLevel = 0;

        // move rect past the label
        var lw = U.labelWidth + Theme.Gap1;
        r.x += lw;
        r.width -= lw;

        // draw the type input
        var rt = r;
        rt.width = k_TypeInputWidth;
        E.PropertyField(rt, type, new GUIContent(""));

        // move past the type input
        var delta = rt.width + Theme.Gap3;
        r.x += delta;
        r.width -= delta;

        // draw the curve input
        var curveType = (AnyCurve.Variant)type.intValue;
        switch (curveType) {
        case AnyCurve.Variant.Map:
            MapCurveDrawer.DrawInput(r, src, dst, curve);
            break;
        case AnyCurve.Variant.MapIn:
            MapInCurveDrawer.DrawInput(r, src, curve);
            break;
        case AnyCurve.Variant.MapOut:
            MapOutCurveDrawer.DrawInput(r, src, curve);
            break;
        case AnyCurve.Variant.Animation:
            curve.animationCurveValue = E.CurveField(r, curve.animationCurveValue);
            break;
        default:
            throw new NotSupportedException($"no drawer for {curveType}");
        }

        // reset indent level
        E.indentLevel = indent;

        E.EndProperty();
    }
}

}