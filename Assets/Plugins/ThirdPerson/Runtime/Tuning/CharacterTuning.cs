using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {

[CreateAssetMenu(fileName = "CharacterTuning", menuName = "thirdperson/CharacterTuning", order = 0)]
public sealed class CharacterTuning: ScriptableObject {
    // -- metadata --
    [Header("metadata")]
    [Tooltip("a friendly description for this config")]
    [TextArea(3, 6)]
    [SerializeField] string m_Description;

    // -- movement system --
    [Header("movement system")]
    [Tooltip("the horizontal speed at which the character stops")]
    public float Horizontal_MinSpeed;

    /// the character's theoretical max horizontal speed
    public float Horizontal_MaxSpeed {
        get => Mathf.Sqrt(Mathf.Max(0.0f, Horizontal_Acceleration - Friction_Kinetic) / Friction_SurfaceDrag);
    }

    [Tooltip("the acceleration from 0 to max speed in units")]
    public float Horizontal_Acceleration;

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
    public float PivotDeceleration {
        get => TimeToPivot > 0 ? Horizontal_MaxSpeed / TimeToPivot : float.PositiveInfinity;
    }

    public float PivotSqrSpeedThreshold {
        get => PivotSpeedThreshold * PivotSpeedThreshold;
    }

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

    public float Crouch_Acceleration {
        get => Crouch_Gravity - Gravity;
    }

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
    public int JumpBuffer;

    [Tooltip("max number of frames the character can be in the air and still jump")]
    public int MaxCoyoteFrames;

    [Tooltip("the gravity while holding jump and moving up")]
    public float JumpGravity;

    /// the vertical acceleration while holding jump and moving up
    public float JumpAcceleration {
        get => JumpGravity - Gravity;
    }

    [Tooltip("the maximum ground angle for jumping")]
    public float Jump_GroundAngle;

    [FormerlySerializedAs("Jump_GroundAngleScale")]
    [Tooltip("the jump scale as a fn of surface angle")]
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
        public int Count = 1;

        [Tooltip("how long after this jump the character can jump again")]
        public int CooldownFrames;

        [Tooltip("the min number of frames jump squat lasts")]
        public int MinJumpSquatFrames = 5;

        [Tooltip("the max number of frames jump squat lasts")]
        public int MaxJumpSquatFrames = 5;

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

    // -- surface --
    [Header("surface")]
    [Tooltip("the time for inertia to decay 99% as a fn of surface angle")]
    public MapOutCurve Surface_InertiaDecayTime;

    [FormerlySerializedAs("WallAngleScale")]
    [FormerlySerializedAs("WallAngleScale_New")]
    [Tooltip("the scaling factor of the surface slide as a fn of surface angle")]
    public MapOutCurve Surface_AngleScale;

    [FormerlySerializedAs("WallTransferScale")]
    [FormerlySerializedAs("Surface_TransferAttack")]
    [Tooltip("the scaling factor of the surface transfer as a fn of surface angle change")]
    public MapOutCurve Surface_DeltaScale;

    [FormerlySerializedAs("Surface_TransferDiScale")]
    [FormerlySerializedAs("WallTransferDiScale")]
    [Tooltip("the scaling factor of the wall transfer as a fn of signed input-surface angle")]
    public MapOutCurve Surface_DiScale;

    [FormerlySerializedAs("Surface_TransferDiAngle")]
    [FormerlySerializedAs("WallTransferDiAngle")]
    [Tooltip("the angle to rotate the transfer surface direction as a fn of signed input-surface angle")]
    public MapOutCurve Surface_DiRotation;

    [FormerlySerializedAs("WallMagnet")]
    [Tooltip("the force the surface pulls character in as a fn of surface angle")]
    public MapOutCurve Surface_Grip;

    [Tooltip("the vertical grip scale as a fn of surface angle")]
    public AnimationCurve Surface_VerticalGrip_Scale;

    [Tooltip("the vertical grip on the surface while moving up")]
    public FloatRange Surface_VerticalGrip_Up;

    [Tooltip("the vertical grip on the surface while moving down")]
    public FloatRange Surface_VerticalGrip_Down;

    [Tooltip("the vertical grip on the surface while moving up & holding")]
    public FloatRange Surface_VerticalGrip_UpHold;

    [Tooltip("the vertical grip on the surface while moving down & holding")]
    public FloatRange Surface_VerticalGrip_DownHold;

    [Tooltip("how much upwards velocity we add to our velocity projection tangent as a fn of surface angle")]
    public MapOutCurve Surface_UpwardsVelocityBias;

    [Tooltip("the time it takes the perceived surface to scale between 0 & 1")]
    public float Surface_PerceptionDuration;

    [Tooltip("the speed the perceived surface's normal moves towards a new normal")]
    public float Surface_PerceptionAngularSpeed;

    // -- friction system --
    [Header("friction system")]
    [Tooltip("the quadratic drag in the air")]
    public float Friction_AerialDrag;

    [FormerlySerializedAs("Horizontal_Drag")]
    [Tooltip("the quadratic drag on a surface")]
    public float Friction_SurfaceDrag;

    [Tooltip("the friction scale as a fn of the surface angle")]
    public MapOutCurve Friction_SurfaceDragScale;

    [FormerlySerializedAs("Horizontal_StaticFriction")]
    [Tooltip("the coefficient of friction when not moving")]
    public float Friction_Static;

    [FormerlySerializedAs("Horizontal_KineticFriction")]
    [Tooltip("the coefficient of friction when moving")]
    public float Friction_Kinetic;

    [FormerlySerializedAs("Friction_SurfaceScale")]
    [FormerlySerializedAs("Surface_FrictionScale")]
    [Tooltip("the friction scale as a fn of the surface angle")]
    public MapOutCurve Friction_SurfaceFrictionScale;

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
}

}