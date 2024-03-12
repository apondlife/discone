using System;
using UnityEngine;

namespace Discone {

/// an attribute that validates a field against a list of values. either pass
/// values as varargs or as a type that has a static field for `string[] Values`
///
/// [FromList("one", "two")]
/// [FromList(typeof MyValue)]
public class FromListAttribute: PropertyAttribute {
    // -- props --
    /// .
    string[] m_List;

    // -- lifetime --
    public FromListAttribute(params string[] list) {
        m_List = list;
    }

    public FromListAttribute(Type type) {
        var values = type.GetField("Values");
        if (values != null) {
            m_List = values.GetValue(null) as string[];
        } else {
            Log.Editor.E($"FromList requires a type implement: static readonly string[] Values;");
        }
    }

    // -- queries --
    /// .
    public string[] List {
        get => m_List;
    }
}

}