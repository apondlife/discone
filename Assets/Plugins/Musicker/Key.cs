namespace Musicker {

/// a western-musical key
public readonly struct Key {
    // -- props --
    /// the root tone for this key
    readonly Tone m_Root;

    // -- lifetime --
    /// create a new key with the root
    public Key(in Tone root) {
        m_Root = root;
    }

    public Key(in Root root) {
        m_Root = new Tone((int)root);
    }

    // -- queries --
    /// transpose tone to this key
    public Tone Transpose(in Tone tone) {
        return tone.Transpose(m_Root);
    }
}

}