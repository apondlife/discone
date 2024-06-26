using System.Reflection;
using UnityEditor;

namespace Soil.Editor {

public static class SerializedPropertyExt {
    // -- commands --
    /// set the float value for the relative property
    public static void SetValue(
        this SerializedProperty prop,
        string path,
        float value
    ) {
        prop.FindPropertyRelative(path).floatValue = value;
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
}

}