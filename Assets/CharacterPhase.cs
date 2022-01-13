using System;

/// a character phase, a composition of an enter, update, and exit action
public readonly struct CharacterPhase: IEquatable<CharacterPhase> {
    // -- props --
    /// a unique name for this phase
    readonly public string Name;

    /// the action to call the phase the frame starts
    readonly public Action Enter;

    /// the action to call every frame the phase is active
    readonly public Action Update;

    /// the action to call the frame the phase ends
    readonly public Action Exit;

    // -- lifetime --
    public CharacterPhase(string name, Action enter = null, Action update = null, Action exit = null) {
        Name = name;
        Enter = enter ?? NoOp;
        Update = update ?? NoOp;
        Exit = exit ?? NoOp;
    }

    // -- commands --
    /// does nothing
    private static void NoOp() {

    }

    // -- IEquatable --
    public bool Equals(CharacterPhase phase) {
            UnityEngine.Debug.Log("Comparing phases raw");
        return Name == phase.Name;
    }

    override public bool Equals(Object obj) {
        if (obj is CharacterPhase phase) {
            UnityEngine.Debug.Log("Comparing phases");
            return Equals(phase);
        } else {
            UnityEngine.Debug.Log("Comparing objects");
            return false;
        }
    }
}
