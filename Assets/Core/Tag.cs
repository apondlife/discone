namespace Discone {

/// a log tag
partial record Tag {
    // -- props --
    /// the name of the tag
    string m_Name;

    /// the color of the tag
    string m_Color;

    // -- lifetime --
    Tag(string name, string color) {
        m_Name = name;
        m_Color = color;
    }

    // -- queries --
    public string F(string message) {
        return $"<color={m_Color}>[{m_Name}]</color> {message}";
    }
}

}
