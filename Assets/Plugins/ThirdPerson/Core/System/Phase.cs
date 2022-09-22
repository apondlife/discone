using System;

namespace ThirdPerson {

/// a character phase, a composition of an enter, update, and exit action
[Serializable]
public struct Phase: IEquatable<Phase> {
    // -- props --
    /// a unique name for this phase
    public string Name;

    /// the action to call the phase the frame starts
    readonly public Action Enter;

    /// the action to call every frame the phase is active
    readonly public Action<float> Update;

    /// the action to call the frame the phase ends
    readonly public Action Exit;

    // -- lifetime --
    public Phase(
        string name,
        Action enter = null,
        Action<float> update = null,
        Action exit = null
    ) {
        Name = name;
        Enter = enter ?? NoOp;
        Update = update ?? NoOp;
        Exit = exit ?? NoOp;
    }

    // -- commands --
    /// does nothing
    private static void NoOp() {
    }

    /// does nothing
    private static void NoOp(float _) {
    }

    // -- IEquatable --
    public bool Equals(Phase phase) {
        return Name == phase.Name;
    }

    override public bool Equals(Object obj) {
        if (obj is Phase phase) {
            return Equals(phase);
        } else {
            return false;
        }
    }

    override public int GetHashCode() {
        return Name.GetHashCode();
    }
}

}