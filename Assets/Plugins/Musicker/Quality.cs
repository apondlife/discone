namespace Musicker {

/// a chord quality
public readonly struct Quality {
    // -- props --
    /// the tones in this quality
    readonly Tone[] m_Tones;

    // -- lifetime --
    /// create a quality w/ the tones
    public Quality(params Tone[] tones) {
        m_Tones = tones;
    }

    // -- queries --
    /// the number of tones in he quality
    public int Length {
        get => m_Tones.Length;
    }

    /// the tone at the position
    public Tone this[int i] {
        get => m_Tones[i];
    }

    // -- factories --
    /// a minor fifth
    public static Quality Min5 = new Quality(
        Tone.I,
        Tone.III.Flat(),
        Tone.V
    );

    /// a major fifth
    public static Quality Maj5 = new Quality(
        Tone.I,
        Tone.III,
        Tone.V
    );

    /// a major 7th chord quality
    public static Quality Maj7 = new Quality(
        Tone.I,
        Tone.III,
        Tone.V,
        Tone.VII
    );

    /// a dominant 7th chord quality
    public static Quality Dom7 = new Quality(
        Tone.I,
        Tone.III,
        Tone.V,
        Tone.VII.Flat()
    );

    /// a dominant 7th chord quality
    public static Quality Min7 = new Quality(
        Tone.I,
        Tone.III.Flat(),
        Tone.V,
        Tone.VII.Flat()
    );

    /// a half-diminished, "minor 7 flat 5", chord quality
    public static Quality Min7Flat5 = new Quality(
        Tone.I,
        Tone.III.Flat(),
        Tone.V.Flat(),
        Tone.VII.Flat()
    );

    /// a diminshed 7th chord quality
    public static Quality Dim7 = new Quality(
        Tone.I,
        Tone.III.Flat(),
        Tone.V.Flat(),
        Tone.VII.Flat(2)
    );
}

}