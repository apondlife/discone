using System;
using UnityEngine;

namespace ThirdPerson {

[CreateAssetMenu(fileName = "CharacterTuning", menuName = "thirdperson/CharacterTuning", order = 0)]
public sealed class CharacterTuning: ScriptableObject {
    // -- metadata --
    [Header("metadata")]
    [Tooltip("a friendly description for this config")]
    [TextArea(3, 6)]
    [SerializeField] string m_Description;

    // -- collision system --
    [Header("collision system")]
    [Tooltip("the speed the perceived surface's normal moves towards a zero")]
    public float Surface_PerceptionLengthSpeed;

    [Tooltip("the speed the perceived surface's normal moves towards a new normal")]
    public float Surface_PerceptionAngularSpeed;

    [Tooltip("a geometric decay on the character's momentum into a surface")]
    public float Surface_MomentumDecay;

    // -- movement system --
    [Header("movement system")]
    [Tooltip("the horizontal speed at which the character stops")]
    public float Horizontal_MinSpeed;

    /// the character's theoretical max horizontal speed
    public float Horizontal_MaxSpeed
        => Mathf.Sqrt(Mathf.Max(0.0f, Horizontal_Acceleration - Horizontal_KineticFriction) / Horizontal_Drag);

    [Tooltip("the acceleration from 0 to max speed in units")]
    public float Horizontal_Acceleration;

    [Tooltip("the time to stop from max speed")]
    public float Horizontal_Drag;

    [Tooltip("the coefficient of friction when not moving")]
    public float Horizontal_StaticFriction;

    [Tooltip("the coefficient of friction when moving")]
    public float Horizontal_KineticFriction;

    [Tooltip("the friction scale as a fn of the surface angle")]
    public MapOutCurve Surface_FrictionScale;

    /// the time to to reach max speed from zero.
    public float TimeToMaxSpeed => TimeToPercentMaxSpeed(
        (Horizontal_MaxSpeed - Horizontal_MinSpeed) / Horizontal_MaxSpeed
    );

    /// the time to stop from max speed
    public float TimeToStop => TimeToPercentMaxSpeed(
        Horizontal_MinSpeed / Horizontal_MaxSpeed
    );

    [Tooltip("the turn speed in radians")]
    public float TurnSpeed;

    [Tooltip("the pivot speed in radians")]
    public float PivotSpeed;

    [Tooltip("the time to finish the pivot deceleration from max speed")]
    public float TimeToPivot;

    [Tooltip("the pivot start threshold, facing • input dir (-1.0, 1.0f)")]
    public float PivotStartThreshold;

    [Tooltip("the minimum speed to be able to pivot")]
    public float PivotSpeedThreshold;

    /// the deceleration of the character while pivoting
    public float PivotDeceleration => TimeToPivot > 0 ? Horizontal_MaxSpeed / TimeToPivot : float.PositiveInfinity;

    public float PivotSqrSpeedThreshold => PivotSpeedThreshold * PivotSpeedThreshold;

    [Tooltip("the turn speed while airborne")]
    public float Air_TurnSpeed;

    [Tooltip("the planar acceleration while floating")]
    public float AerialDriftAcceleration;

    // -- crouch system --
    [Header("crouch system")]
    [Tooltip("the static friction value when crouching")]
    public float Crouch_StaticFriction;

    [Tooltip("the turn speed while crouching")]
    public float Crouch_TurnSpeed;

    [Tooltip("the max lateral speed when crouching")]
    public float Crouch_LateralMaxSpeed;

    [Tooltip("the turn speed while crouching")]
    public float Crouch_Gravity;

    public float Crouch_Acceleration => Crouch_Gravity - Gravity;

    [Tooltip("the kinetic friction when crouching towards movement")]
    public MapOutCurve Crouch_PositiveKineticFriction;

    [Tooltip("the kinetic friction when crouching against movement")]
    public MapOutCurve Crouch_NegativeKineticFriction;

    [Tooltip("the drag when crouching towards movement")]
    public MapOutCurve Crouch_PositiveDrag;

    [Tooltip("the drag when crouching against movement")]
    public MapOutCurve Crouch_NegativeDrag;

    // -- jump system --
    [Header("jump system")]
    [Tooltip("the acceleration due to gravity")]
    public float Gravity;

    [Tooltip("how many frames you can have pressed jump before landing to execute the jump")]
    public uint JumpBuffer;

    [Tooltip("max number of frames the character can be in the air and still jump")]
    public uint MaxCoyoteFrames;

    [Tooltip("the gravity while holding jump and moving up")]
    public float JumpGravity;

    /// the vertical acceleration while holding jump and moving up
    public float JumpAcceleration {
        get => JumpGravity - Gravity;
    }

    [Tooltip("the maximum ground angle for jumping")]
    public float Jump_GroundAngle;

    [Tooltip("the jump scale as a fn of surface angle")]
    [UnityEngine.Serialization.FormerlySerializedAs("Jump_GroundAngleScale")]
    public MapOutCurve Jump_SurfaceAngleScale;

    [Tooltip("the jump speed opposed to the surface normal as a fn of squat duration")]
    public MapOutCurve Jump_Normal_Speed;

    [Tooltip("the jump scale opposed to the surface normal as a fn of surface angle")]
    public MapOutCurve Jump_Normal_SurfaceAngleScale;

    [Tooltip("the gravity while holding jump and falling")]
    public float FallGravity;

    /// the vertical acceleration while holding jump and falling
    public float FallAcceleration {
        get => FallGravity - Gravity;
    }

    [Tooltip("how long the landing state lasts when falling")]
    public float Landing_Duration;

    [Tooltip("the tuning for each jump, sequentially")]
    public JumpTuning[] Jumps;

    [Serializable]
    public class JumpTuning {
        [Tooltip("the number of times this jump can be executed; 0 = infinite")]
        public uint Count = 1;

        [Tooltip("how long after this jump the character can jump again")]
        public uint CooldownFrames;

        [Tooltip("the min number of frames jump squat lasts")]
        public uint MinJumpSquatFrames = 5;

        [Tooltip("the max number of frames jump squat lasts")]
        public uint MaxJumpSquatFrames = 5;

        // TODO: convert to map out curve & remember how to propely update all prefabs
        [Tooltip("the minimum jump speed (minimum length jump squat)")]
        public float Vertical_MinSpeed;

        [Tooltip("the maximum jump speed (maximum length jump squat)")]
        public float Vertical_MaxSpeed;

        [Tooltip("jump speed as a fn of squat duration")]
        public AnimationCurve Vertical_SpeedCurve;

        [Tooltip("how much upwards speed is cancelled on jump")]
        public float Upwards_MomentumLoss;

        [Tooltip("how much horizontal speed is cancelled on jump")]
        public float Horizontal_MomentumLoss;
    }

    // -- wall --
    [Header("wall")]
    [Tooltip("the collision layer of what counts as walls for wall sliding")]
    public LayerMask WallLayer;

    [Tooltip("the scaling factor of the wall slide as a fn of surface angle")]
    [UnityEngine.Serialization.FormerlySerializedAs("WallAngleScale_New")]
    public MapOutCurve WallAngleScale;

    [Tooltip("the gravity while on the wall & holding jump")]
    public AdsrCurve WallGravity;

    [Tooltip("the gravity while on the wall & not holding jump")]
    public AdsrCurve WallHoldGravity;

    [Tooltip("the force the surface pulls character in as a fn of surface angle")]
    public MapOutCurve WallMagnet;

    [Tooltip("the scaling factor on the wall magnet as a fn of the input • wall forward")]
    public MapOutCurve WallMagnetInputScale;

    [Tooltip("the scaling factor on the wall magnet as a fn of perception delta")]
    public MapOutCurve WallMagnetTransferScale;

    // TODO: these are currently mirrored around 90; they should curved over 180
    // so that's not necessarily the case
    [Tooltip("the scaling factor of the wall transfer as a fn of surface change angle")]
    public MapOutCurve WallTransferScale;

    [Tooltip("the angle to rotate the transfer surface direction as a fn of signed input-surface angle")]
    public MapOutCurve WallTransferDiAngle;

    [Tooltip("the scaling factor of the wall transfer as a fn of signed input-surface angle")]
    public MapOutCurve WallTransferDiScale;

    [Tooltip("the scaling factor of the wall gravity amplitude as a fn of surface change angle")]
    public MapOutCurve WallGravityAmplitudeScale;

    public float WallAcceleration(float wallGravity) {
        return wallGravity - Gravity + FallGravity;
    }

    // -- model/animation --
    [Header("model / animation")]
    [Tooltip("the angle in degrees character model tilts forward on the start up acceleration")]
    public float TiltForBaseAcceleration;

    [Tooltip("the maximum angle in degrees the character can tilt")]
    public float MaxTilt;

    [Tooltip("the smoothing on the character tilt")]
    public float TiltSmoothing;

    // -- lifecycle --
    void OnValidate() {
        if (Jumps == null || Jumps.Length == 0) {
            MaxCoyoteFrames = 0;
        } else {
            MaxCoyoteFrames = Math.Max(MaxCoyoteFrames, Jumps[0].MinJumpSquatFrames);
        }
    }

    // -- queries --
    public float TimeToPercentMaxSpeed(float pct) {
        return -Mathf.Log(1.0f - pct, (float)Math.E) / Horizontal_Drag;
    }
}

}