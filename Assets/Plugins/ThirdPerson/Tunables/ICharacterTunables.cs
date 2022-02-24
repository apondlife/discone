using System;
using UnityEngine;

namespace ThirdPerson {

// https://www.patrykgalach.com/2020/01/27/assigning-interface-in-unity-inspector/
public interface ICharacterTunables {
    // -- gravity --
    float Gravity { get; }

    // -- movement --
    float MaxPlanarSpeed { get; }
    float MinPlanarSpeed { get; }
    float Acceleration { get; }
    float TimeToMaxSpeed { get; }
    float Deceleration { get; }
    float TimeToStop { get; }
    float TurnSpeed { get; }
    float PivotSpeed { get; }
    float PivotDeceleration { get; }
    float PivotStartThreshold { get; }
    float TurningFriction { get; }
    float TimeToPivot { get; }

    // -- air movement --
    float FloatAcceleration { get; }

    // -- jump --
    uint JumpBuffer { get; }
    uint MinJumpSquatFrames { get; }
    uint MaxJumpSquatFrames { get; }
    uint MaxCoyoteFrames { get; }
    float MinJumpSpeed { get; }
    float MaxJumpSpeed { get; }
    AnimationCurve JumpSpeedCurve { get; }
    float MinJumpHeight { get; }
    float MaxJumpHeight { get; }
    float JumpGravity { get; }
    float JumpAcceleration { get; }
    float FallGravity { get; }
    float FallAcceleration { get; }

    // -- wall --
    LayerMask WallLayer { get; }
    float WallGravity { get; }
    float WallAcceleration { get; }

    // -- tilt --
    float TiltForBaseAcceleration { get; }
    float MaxTilt { get; }
    float TiltSmoothing { get; }

    // -- camera --
    float DutchScale { get; }
    float DutchSmoothing { get; }
    float Damping { get; }
    float FastDamping { get; }
}

}