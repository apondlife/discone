using UnityEngine;

namespace ThirdPerson {

// https://www.patrykgalach.com/2020/01/27/assigning-interface-in-unity-inspector/
public abstract class CharacterTunablesBase: ScriptableObject {
    // -- gravity --
    public abstract float Gravity { get; }

    // -- movement --
    public abstract float MaxPlanarSpeed { get; }
    public abstract float MinPlanarSpeed { get; }
    public abstract float Acceleration { get; }
    public abstract float TimeToMaxSpeed { get; }
    public abstract float Deceleration { get; }
    public abstract float TimeToStop { get; }
    public abstract float TurnSpeed { get; }
    public abstract float PivotSpeed { get; }
    public abstract float PivotDeceleration { get; }
    public abstract float PivotStartThreshold { get; }
    public abstract float TurningFriction { get; }
    public abstract float TimeToPivot { get; }

    // -- air movement --
    public abstract float AerialDriftAcceleration { get; }

    // -- jump --
    public abstract uint JumpBuffer { get; }
    public abstract uint MinJumpSquatFrames { get; }
    public abstract uint MaxJumpSquatFrames { get; }
    public abstract uint MaxCoyoteFrames { get; }
    public abstract float MinJumpSpeed { get; }
    public abstract float MaxJumpSpeed { get; }
    public abstract AnimationCurve JumpSpeedCurve { get; }
    public abstract float MinJumpHeight { get; }
    public abstract float MaxJumpHeight { get; }
    public abstract float JumpGravity { get; }
    public abstract float JumpAcceleration { get; }
    public abstract float FallGravity { get; }
    public abstract float FallAcceleration { get; }
    public abstract float MinJumpSpeed_Horizontal { get; }
    public abstract float MaxJumpSpeed_Horizontal { get; }
    public abstract AnimationCurve JumpSpeedCurve_Horizontal { get; }

    // -- wall --
    public abstract LayerMask WallLayer { get; }
    public abstract float WallGravity { get; }
    public abstract float WallAcceleration { get; }

    // -- tilt --
    public abstract float TiltForBaseAcceleration { get; }
    public abstract float MaxTilt { get; }
    public abstract float TiltSmoothing { get; }

    // -- camera --
    public abstract float DutchScale { get; }
    public abstract float DutchSmoothing { get; }
    public abstract float Damping { get; }
    public abstract float FastDamping { get; }
}

}