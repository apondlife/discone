using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Soil.Editor {

public static class SerializedPropertyExt {
    // -- commands --
    /// set the float value for the relative property
    public static void SetValue(
        this SerializedProperty prop,
        string path,
        float value,
        bool isPrivate = false
    ) {
        prop.FindProp(path, isPrivate).floatValue = value;
    }

    /// set the animation curve value for the relative property
    public static void SetValue(
        this SerializedProperty prop,
        string path,
        AnimationCurve value,
        bool isPrivate = false
    ) {
        prop.FindProp(path, isPrivate).animationCurveValue = value;
    }

    // -- queries --
    /// find the relative property by path; prefixes the name of private props
    public static SerializedProperty FindProp(
        this SerializedProperty prop,
        string path,
        bool isPrivate = false
    ) {
        return prop.FindPropertyRelative(isPrivate ? $"m_{path}" : path);
    }

    /// find the associated value-typed property for a serialized property
    public static bool FindValue<T>(
        this SerializedProperty prop,
        out T value,
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic
    ) where T: struct {
        var owner = prop.serializedObject.targetObject;
        var ownerType = owner.GetType();

        var field = ownerType.GetField(prop.name, flags);
        if (field == null) {
            value = default;
            return false;
        }

        if (field.GetValue(owner) is not T inner) {
            value = default;
            return false;
        }

        value = inner;
        return true;
    }

    /// find the field attribute, if any
    public static T FindAttribute<T>(
        this SerializedProperty prop
    ) where T: Attribute {
        return prop.FindFieldInfo().GetCustomAttribute<T>();
    }

    /// find the field attributes, if any
    public static IEnumerable<T> FindAttributes<T>(
        this SerializedProperty prop
    ) where T: Attribute {
        return prop.FindFieldInfo().GetCustomAttributes<T>();
    }

    /// find the field info for the property
    public static FieldInfo FindFieldInfo(this SerializedProperty prop) {
        var currFieldInfo = null as FieldInfo;

        // given the path to the field
        var path = prop.propertyPath;
        var pathLen = path.Length;

        // given the current position in the path / tree
        var currIndex = 0;
        var currType = prop.serializedObject.targetObject.GetType();

        // pick nested fields until we consume the entire path
        while (currIndex < pathLen) {
            var nextIndex = path.IndexOf('.', currIndex);
            if (nextIndex == -1) {
                nextIndex = pathLen;
            }

            var nextType = currType;
            var nextFieldInfo = currFieldInfo;

            // if this component is an array, use the nested type
            var name = path.Substring(currIndex, nextIndex - currIndex);
            if (name == "Array") {
                nextType = currType.GetElementType();
            }
            // otherwise, it's a field (skip the "data[i]" component that follows "Array")
            else if (!name.StartsWith("data[")) {
                var fieldInfo = currType.GetField(
                    name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );

                nextType = fieldInfo.FieldType;
                nextFieldInfo = fieldInfo;
            }

            currIndex = nextIndex + 1;
            currType = nextType;
            currFieldInfo = nextFieldInfo;
        }

        return currFieldInfo;
    }
}

}