using System;

namespace Soil {

/// a character phase, a composition of an enter, update, and exit action
[Serializable]
public struct Phase: IEquatable<Phase> {
    // -- props --
    /// a unique name for this phase
    public string Name;

    /// the action to call the phase the frame starts
    public readonly Action Enter;

    /// the action to call every frame the phase is active
    public readonly Action<float> Update;

    /// the action to call the frame the phase ends
    public readonly Action Exit;

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
    static void NoOp() {
    }

    /// does nothing
    static void NoOp(float _) {
    }

    // -- IEquatable --
    public bool Equals(Phase phase) {
        return Name == phase.Name;
    }

    public override bool Equals(Object obj) {
        if (obj is Phase phase) {
            return Equals(phase);
        } else {
            return false;
        }
    }

    public override int GetHashCode() {
        return Name.GetHashCode();
    }
}

}