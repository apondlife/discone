using UnityEngine;

// https://www.patrykgalach.com/2020/01/27/assigning-interface-in-unity-inspector/
public abstract class CharacterTunablesBase: ScriptableObject {
    // -- movement --
    public abstract float MaxPlanarSpeed { get; } //base speed? speed?
    public abstract float TurnSpeed { get; } //base speed? speed?
    public abstract float PivotSpeed { get; }
    public abstract float PivotDeceleration { get; }
    public abstract float PivotStartThreshold { get; }
    public abstract float TimeToPivot { get; }
    public abstract float Gravity { get; }
    public abstract float TimeToMaxSpeed { get; }
    public abstract float TimeToStop { get; }
    public abstract float Acceleration { get; }
    public abstract float Deceleration { get; }

    // -- air movement

    // -- jump --
    public abstract float InitialJumpSpeed { get; }
    public abstract int JumpSquatFrames { get; }
    public abstract float MinJumpSpeed { get; }
    public abstract float MaxJumpSpeed { get; }
    public abstract AnimationCurve JumpSpeedCurve { get; }
    public abstract float JumpAcceleration { get; }
    public abstract float FloatAcceleration { get; }

    // -- tilt --
    public abstract float TiltForBaseAcceleration { get; }
    public abstract float MaxTilt { get; }
    public abstract float TiltSmoothing { get; }

    // -- camera --
    public abstract float DutchScale { get; }
    public abstract float DutchSmoothing { get; }
}

