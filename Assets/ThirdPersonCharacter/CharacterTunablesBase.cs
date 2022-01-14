using UnityEngine;

// https://www.patrykgalach.com/2020/01/27/assigning-interface-in-unity-inspector/
public abstract class CharacterTunablesBase: ScriptableObject {
    public abstract float MaxPlanarSpeed { get; } //base speed? speed?
    public abstract float TurnSpeed { get; } //base speed? speed?
    public abstract float Gravity { get; }
    public abstract float InitialJumpSpeed { get; }
    public abstract float TimeToMaxSpeed { get; }
    public abstract float TimeToStop { get; }
    public abstract float Acceleration { get; }
    public abstract float Deceleration { get; }
}

