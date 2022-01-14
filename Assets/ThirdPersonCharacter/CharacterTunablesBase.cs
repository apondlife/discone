using UnityEngine;

public abstract class CharacterTunablesBase : ScriptableObject {
    public abstract float PlanarSpeed { get; } //base speed? speed?
    public abstract float Gravity { get; }
    public abstract float InitialJumpSpeed { get; }
}

