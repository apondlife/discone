using UnityEngine;

namespace ThirdPerson {

public sealed class CharacterTunablesProvider : ICharacterTunables {
    [Header("metadata")]
    [SerializeField] CharacterTunables tunables;

    // -- gravity --
    public float Gravity => tunables.Gravity;
    // -- movement --
    public float MaxPlanarSpeed => tunables.MaxPlanarSpeed;
    public float MinPlanarSpeed => tunables.MinPlanarSpeed;
    public float Acceleration => tunables.Acceleration;
    public float TimeToMaxSpeed => tunables.TimeToMaxSpeed;
    public float Deceleration => tunables.Deceleration;
    public float TimeToStop => tunables.TimeToStop;
    public float TurnSpeed => tunables.TurnSpeed;
    public float PivotSpeed => tunables.PivotSpeed;
    public float PivotDeceleration => tunables.PivotDeceleration;
    public float PivotStartThreshold => tunables.PivotStartThreshold;
    public float TurningFriction => tunables.TurningFriction;
    public float TimeToPivot => tunables.TimeToPivot;

    // -- air movement --
    public float FloatAcceleration => tunables.FloatAcceleration;

    // -- jump --
    public uint JumpBuffer => tunables.JumpBuffer;
    public uint MinJumpSquatFrames => tunables.MinJumpSquatFrames;
    public uint MaxJumpSquatFrames => tunables.MaxJumpSquatFrames;
    public uint MaxCoyoteFrames => tunables.MaxCoyoteFrames;
    public float MinJumpSpeed => tunables.MinJumpSpeed;
    public float MaxJumpSpeed => tunables.MaxJumpSpeed;
    public AnimationCurve JumpSpeedCurve => tunables.JumpSpeedCurve;
    public float MinJumpHeight => tunables.MinJumpHeight;
    public float MaxJumpHeight => tunables.MaxJumpHeight;
    public float JumpGravity => tunables.JumpGravity;
    public float JumpAcceleration => tunables.JumpAcceleration;
    public float FallGravity => tunables.FallGravity;
    public float FallAcceleration => tunables.FallAcceleration;

    // -- wall --
    public LayerMask WallLayer => tunables.WallLayer;
    public float WallGravity => tunables.WallGravity;
    public float WallAcceleration => tunables.WallAcceleration;

    // -- tilt --
    public float TiltForBaseAcceleration => tunables.TiltForBaseAcceleration;
    public float MaxTilt => tunables.MaxTilt;
    public float TiltSmoothing => tunables.TiltSmoothing;

    // -- camera --
    public float DutchScale => tunables.DutchScale;
    public float DutchSmoothing => tunables.DutchSmoothing;
    public float Damping => tunables.Damping;
    public float FastDamping => tunables.FastDamping;
}
}