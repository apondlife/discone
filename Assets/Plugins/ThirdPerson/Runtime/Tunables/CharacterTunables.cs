using System;
using UnityEngine;

namespace ThirdPerson {

[CreateAssetMenu(fileName = "CharacterTunables", menuName = "thirdperson/CharacterTunables", order = 0)]
public sealed class CharacterTunables: ScriptableObject {
    // -- metadata --
    [Header("metadata")]
    [Tooltip("a friendly description for this config")]
    [TextArea(3, 6)]
    [SerializeField] private string m_Description;

    // -- movement system --
    [Header("movement system")]
    [Tooltip("the horizontal speed at which the character stops")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_MinPlanarSpeed")]
    [SerializeField] float m_Horizontal_MinSpeed;
    public float Horizontal_MinSpeed => m_Horizontal_MinSpeed;

    /// the character's theoretical max horizontal speed
    public float Horizontal_MaxSpeed
        => Mathf.Sqrt(Mathf.Max(0.0f, Horizontal_Acceleration - Horizontal_KineticFriction) / Horizontal_Drag);

    [Tooltip("the acceleration from 0 to max speed in units")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_Acceleration")]
    [SerializeField] private float m_Horizontal_Acceleration;
    public float Horizontal_Acceleration => m_Horizontal_Acceleration;

    /// the deceleration from 0 to max speed in units
    [Tooltip("the time to stop from max speed")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_Deceleration")]
    [SerializeField] private float m_Horizontal_Drag;
    public float Horizontal_Drag => m_Horizontal_Drag;

    [Tooltip("the coefficient of friction when not moving")]
    [SerializeField] private float m_Horizontal_StaticFriction;
    public float Horizontal_StaticFriction => m_Horizontal_StaticFriction;

    [Tooltip("the coefficient of friction when moving")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_TurningFriction")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_Horizontal_Friction")]
    [SerializeField] private float m_Horizontal_KineticFriction;
    public float Horizontal_KineticFriction => m_Horizontal_KineticFriction;

    /// the time to to reach max speed from zero.
    public float TimeToMaxSpeed => TimeToPercentMaxSpeed(
        (Horizontal_MaxSpeed - Horizontal_MinSpeed) / Horizontal_MaxSpeed
    );

    /// the time to stop from max speed
    public float TimeToStop => TimeToPercentMaxSpeed(
        Horizontal_MinSpeed / Horizontal_MaxSpeed
    );

    [Tooltip("the turn speed in radians")]
    [SerializeField] private float m_TurnSpeed;
    public float TurnSpeed => m_TurnSpeed;

    [Tooltip("the pivot speed in radians")]
    [SerializeField] private float m_PivotSpeed;
    public float PivotSpeed => m_PivotSpeed;

    [Tooltip("the time to finish the pivot deceleration from max speed")]
    [SerializeField] private float m_TimeToPivot;
    public float TimeToPivot => m_TimeToPivot;

    [Tooltip("the pivot start threshold, facing • input dir (-1.0, 1.0f)")]
    [SerializeField] private float m_PivotStartThreshold;
    public float PivotStartThreshold => m_PivotStartThreshold;

    [Tooltip("the minimum speed to be able to pivot")]
    [SerializeField] private float m_PivotSpeedThreshold;
    public float PivotSpeedThreshold => m_PivotSpeedThreshold;

    /// the deceleration of the character while pivoting
    public float PivotDeceleration => TimeToPivot > 0 ? Horizontal_MaxSpeed / TimeToPivot : float.PositiveInfinity;

    public float PivotSqrSpeedThreshold => PivotSpeedThreshold * PivotSpeedThreshold;

    [Tooltip("the turn speed while airborne")]
    [SerializeField] float m_Air_TurnSpeed;
    public float Air_TurnSpeed => m_Air_TurnSpeed;

    [Tooltip("the planar acceleration while floating")]
    [SerializeField] float m_AerialDriftAcceleration;
    public float AerialDriftAcceleration => m_AerialDriftAcceleration;

    // -- crouch system --
    [Header("crouch system")]
    [Tooltip("the static friction value when crouching")]
    [SerializeField] private float m_Crouch_StaticFriction;
    public float Crouch_StaticFriction => m_Crouch_StaticFriction;

    [Tooltip("the turn speed while crouching")]
    [SerializeField] float m_Crouch_TurnSpeed;
    public float Crouch_TurnSpeed => m_Crouch_TurnSpeed;

    [Tooltip("the max lateral speed when crouching")]
    [SerializeField] private float m_Crouch_LateralMaxSpeed;
    public float Crouch_LateralMaxSpeed => m_Crouch_LateralMaxSpeed;

    [Tooltip("the turn speed while crouching")]
    [SerializeField] float m_Crouch_Gravity;
    public float Crouch_Gravity => m_Crouch_Gravity;

    public float Crouch_Acceleration => Crouch_Gravity - Gravity;

    [Tooltip("the kinetic friction when crouching towards movement")]
    [SerializeField] private MapOutCurve m_Crouch_PositiveKineticFriction;
    public MapOutCurve Crouch_PositiveKineticFriction => m_Crouch_PositiveKineticFriction;

    [Tooltip("the kinetic friction when crouching against movement")]
    [SerializeField] private MapOutCurve m_Crouch_NegativeKineticFriction;
    public MapOutCurve Crouch_NegativeKineticFriction => m_Crouch_NegativeKineticFriction;

    [Tooltip("the drag when crouching towards movement")]
    [SerializeField] private MapOutCurve m_Crouch_PositiveDrag;
    public MapOutCurve Crouch_PositiveDrag => m_Crouch_PositiveDrag;

    [Tooltip("the drag when crouching against movement")]
    [SerializeField] private MapOutCurve m_Crouch_NegativeDrag;
    public MapOutCurve Crouch_NegativeDrag => m_Crouch_NegativeDrag;

    // -- jump system --
    [Header("jump system")]
    [Tooltip("the acceleration due to gravity")]
    [SerializeField] private float m_Gravity;
    public float Gravity => m_Gravity;

    [Tooltip("how many frames you can have pressed jump before landing to execute the jump")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_JumpBuffer")]
    [SerializeField] private uint m_JumpBuffer;
    public uint JumpBuffer => m_JumpBuffer;

    [Tooltip("max number of frames the character can be in the air and still jump")]
    [SerializeField] private uint m_MaxCoyoteFrames;
    public uint MaxCoyoteFrames => m_MaxCoyoteFrames;

    [Tooltip("the gravity while holding jump and moving up")]
    [SerializeField] private float m_JumpGravity;
    public float JumpGravity => m_JumpGravity;

    /// the vertical acceleration while holding jump and moving up
    public float JumpAcceleration {
        get => m_JumpGravity - m_Gravity;
    }

    [Tooltip("the gravity while holding jump and falling")]
    [SerializeField] private float m_FallGravity;
    public float FallGravity => m_FallGravity;

    /// the vertical acceleration while holding jump and falling
    public float FallAcceleration {
        get => m_FallGravity - m_Gravity;
    }

    [Tooltip("how long the landing state lasts when falling")]
    [SerializeField] private float m_Landing_Duration;
    public float Landing_Duration => m_Landing_Duration;

    [Tooltip("the tunables for each jump, sequentially")]
    [SerializeField] private JumpTunables[] m_Jumps;
    public JumpTunables[] Jumps => m_Jumps;

    [Serializable]
    public class JumpTunables {
        [Tooltip("the number of times this jump can be executed; 0 = infinite")]
        [SerializeField] private uint m_Count = 1;
        public uint Count => m_Count;

        [Tooltip("how long after this jump the character can jump again")]
        [SerializeField] private uint m_CooldownFrames;
        public uint CooldownFrames => m_CooldownFrames;

        [Tooltip("the min number of frames jump squat lasts")]
        [SerializeField] private uint m_MinJumpSquatFrames = 5;
        public uint MinJumpSquatFrames => m_MinJumpSquatFrames;

        [Tooltip("the max number of frames jump squat lasts")]
        [SerializeField] private uint m_MaxJumpSquatFrames = 5;
        public uint MaxJumpSquatFrames => m_MaxJumpSquatFrames;

        [Tooltip("the minimum jump speed (minimum length jump squat)")]
        [SerializeField] private float m_Vertical_MinSpeed;
        public float Vertical_MinSpeed => m_Vertical_MinSpeed;

        [Tooltip("the maximum jump speed (maximum length jump squat)")]
        [SerializeField] private float m_Vertical_MaxSpeed;
        public float Vertical_MaxSpeed => m_Vertical_MaxSpeed;

        [Tooltip("how the jump speed changes from holding the squat")]
        [SerializeField] private AnimationCurve m_Vertical_SpeedCurve;
        public AnimationCurve Vertical_SpeedCurve => m_Vertical_SpeedCurve;

        [Tooltip("how much upwards speed is cancelled on jump")]
        [SerializeField] private float m_Upwards_MomentumLoss;
        public float Upwards_MomentumLoss => m_Upwards_MomentumLoss;

        [Tooltip("the minimum horizontal jump speed (minimum length jump squat)")]
        [SerializeField] private float m_Horizontal_MinSpeed;
        public float Horizontal_MinSpeed => m_Horizontal_MinSpeed;

        [Tooltip("the maximum horizontal jump speed (maximum length jump squat)")]
        [SerializeField] private float m_Horizontal_MaxSpeed;
        public float Horizontal_MaxSpeed => m_Horizontal_MaxSpeed;

        [Tooltip("how the jump speed changes from holding the squat")]
        [SerializeField] private AnimationCurve m_Horizontal_SpeedCurve;
        public AnimationCurve Horizontal_SpeedCurve => m_Horizontal_SpeedCurve;

        [Tooltip("how much vertical speed is cancelled on jump")]
        [SerializeField] private float m_Horizontal_MomentumLoss;
        public float Horizontal_MomentumLoss => m_Horizontal_MomentumLoss;
    }

    // -- wall --
    [Header("wall")]
    [Tooltip("the collision layer of what counts as walls for wall sliding")]
    [SerializeField] private LayerMask m_WallLayer;
    public LayerMask WallLayer => m_WallLayer;

    [Tooltip("the gravity while on the wall & holding jump")]
    [SerializeField] private AdsrCurve m_WallGravity;
    public AdsrCurve WallGravity => m_WallGravity;

    [Tooltip("the gravity while on the wall & not holding jump")]
    [SerializeField] private AdsrCurve m_WallHoldGravity;
    public AdsrCurve WallHoldGravity => m_WallHoldGravity;

    [Tooltip("the force the wall pull the character to make it stick")]
    [SerializeField] public float m_WallMagnet;
    public float WallMagnet => m_WallMagnet;

    public float WallAcceleration(float wallGravity) {
        return wallGravity - Gravity + FallGravity;
    }

    // -- model/animation --
    [Header("model / animation")]
    [Tooltip("the angle in degrees character model tilts forward on the start up acceleration")]
    [SerializeField] private float m_TiltForBaseAcceleration;
    public float TiltForBaseAcceleration => m_TiltForBaseAcceleration;

    [Tooltip("the maximum angle in degrees the character can tilt")]
    [SerializeField] private float m_MaxTilt;
    public float MaxTilt => m_MaxTilt;

    [Tooltip("the smoothing on the character tilt")]
    [SerializeField] private float m_TiltSmoothing;
    public float TiltSmoothing => m_TiltSmoothing;

    // -- lifecycle --
    void OnValidate() {
        if (m_Jumps == null || m_Jumps.Length == 0) {
            m_MaxCoyoteFrames = 0;
        } else {
            m_MaxCoyoteFrames = Math.Max(m_MaxCoyoteFrames, m_Jumps[0].MinJumpSquatFrames);
        }
    }

    // -- queries --
    public float TimeToPercentMaxSpeed(float pct) {
        return -Mathf.Log(1.0f - pct, (float)Math.E) / Horizontal_Drag;
    }
}

}