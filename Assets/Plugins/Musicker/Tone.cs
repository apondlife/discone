namespace Musicker {

/// a western-musical tone
public readonly struct Tone {
    // -- props --
    /// the number of steps from the root
    readonly int m_Steps;

    // -- lifetime --
    /// create a new tone w/ a number of steps
    public Tone(int steps) {
        m_Steps = steps;
    }

    /// create a new tone w/ an interval
    public Tone(Interval interval) {
        m_Steps = (int)interval;
    }

    // -- operators --
    /// makes the tone n steps sharper
    public Tone Add(int nSteps = 1) {
        return new Tone(m_Steps + nSteps);
    }

    /// alias for Add
    public Tone Sharp(int nSteps = 1) {
        return Add(nSteps);
    }

    /// makes the tone n steps flatter
    public Tone Flat(int nSteps = 1) {
        return new Tone(m_Steps - nSteps);
    }

    /// transpose a tone given the root
    public Tone Transpose(Tone root) {
        return new Tone(root.m_Steps + m_Steps);
    }

    /// changes the tone by n octaves
    public Tone Octave(int nOctaves = 1) {
        return new Tone(m_Steps + nOctaves * 12);
    }

    // -- queries --
    /// the number of steps from the root
    public int Steps {
        get => m_Steps;
    }

    // -- factories --
    public static Tone I {
        get => new Tone(0);
    }

    public static Tone II {
        get => new Tone(2);
    }

    public static Tone III {
        get => new Tone(4);
    }

    public static Tone IV {
        get => new Tone(5);
    }

    public static Tone V {
        get => new Tone(7);
    }

    public static Tone VI {
        get => new Tone(9);
    }

    public static Tone VII {
        get => new Tone(11);
    }
}

}