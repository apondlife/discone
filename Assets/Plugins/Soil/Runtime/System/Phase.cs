using System;

namespace Soil {

/// a character phase, a composition of an enter, update, and exit action
[Serializable]
public struct Phase<Container>: IEquatable<Phase<Container>> {
    // -- props --
    /// a unique name for this phase
    public string Name;

    /// the action to call the phase the frame starts
    public readonly Action<System<Container>, Container> Enter;

    /// the action to call every frame the phase is active
    public readonly Action<float, System<Container>, Container> Update;

    /// the action to call the frame the phase ends
    public readonly Action<System<Container>, Container> Exit;

    // -- lifetime --
    public Phase(
        string name,
        Action<System<Container>, Container> enter = null,
        Action<float, System<Container>, Container> update = null,
        Action<System<Container>, Container> exit = null
    ) {
        Name = name;
        Enter = enter ?? NoOp;
        Update = update ?? NoOp;
        Exit = exit ?? NoOp;
    }

    // -- commands --
    /// does nothing
    static void NoOp(System<Container> _, Container __) {
    }

    /// does nothing
    static void NoOp(float _, System<Container> __, Container ___) {
    }

    // -- IEquatable --
    public bool Equals(Phase<Container> phase) {
        return Name == phase.Name;
    }

    public override bool Equals(Object obj) {
        if (obj is Phase<Container> phase) {
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