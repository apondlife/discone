using UnityEngine;

namespace Soil {

/// a log tag
public record Tag {
    // -- props --
    /// the name of the tag
    string m_Name;

    /// the color of the tag
    string m_Color;

    // -- lifetime --
    public Tag(string name, string color) {
        m_Name = name;
        m_Color = color;
    }

    // -- commands --
    /// log an info message
    public void I(string message) {
        Debug.Log(F(message));
    }

    /// log an warning message
    public void W(string message) {
        Debug.LogWarning(F(message));
    }

    /// log an error message
    public void E(string message) {
        Debug.LogError(F(message));
    }

    // -- queries --
    public string F(string message) {
        return $"<color={m_Color}>[{m_Name}]</color> {message}";
    }
}

}