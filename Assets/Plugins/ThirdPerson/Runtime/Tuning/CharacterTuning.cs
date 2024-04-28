using System;
using Soil;
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

    // -- movement --
    [Header("movement")]
    [FormerlySerializedAs("Horizontal_MinSpeed")]
    [Tooltip("the horizontal speed at which the character stops")]
    public float Surface_MinSpeed;

    /// the character's theoretical max surface speed
    public float Surface_MaxSpeed {
        get => Mathf.Sqrt(Mathf.Max(0.0f, Surface_Acceleration.Evaluate(0f) - Friction_Kinetic) / Friction_SurfaceDrag);
    }

    [Tooltip("the movement acceleration as a fn of surface angle")]
    public MapOutCurve Surface_Acceleration;

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
        get => TimeToPivot > 0 ? Surface_MaxSpeed / TimeToPivot : float.PositiveInfinity;
    }

    public float PivotSqrSpeedThreshold {
        get => PivotSpeedThreshold * PivotSpeedThreshold;
    }

    [Tooltip("the turn speed while airborne")]
    public float Air_TurnSpeed;

    [Tooltip("the planar acceleration while floating")]
    public float AerialDriftAcceleration;

    // -- crouch --
    [Header("crouch")]
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

    // -- jump --
    [Header("jump")]
    [Tooltip("the acceleration due to gravity")]
    public float Gravity;

    [Tooltip("the gravity while holding jump and moving up")]
    public float JumpGravity;

    [Tooltip("the gravity while holding jump and falling")]
    public float FallGravity;

    // TODO: make buffer for release edge as well
    [Tooltip("the duration of the jump buffer")]
    public float Jump_BufferDuration;

    [FormerlySerializedAs("MaxCoyoteTime")]
    [Tooltip("the max time the character can be in the air and still jump")]
    public float CoyoteDuration;

    [FormerlySerializedAs("Landing_Duration")]
    [Tooltip("how long the landing state lasts when falling")]
    public float LandingDuration;

    [FormerlySerializedAs("Jump_GroundAngleScale")]
    [Tooltip("the jump scale as a fn of surface angle")]
    public MapOutCurve Jump_SurfaceAngleScale;

    [Tooltip("the jump speed opposed to the surface normal as a fn of squat duration")]
    public MapOutCurve Jump_Normal_Speed;

    [Tooltip("the jump scale opposed to the surface normal as a fn of surface angle")]
    public MapOutCurve Jump_Normal_SurfaceAngleScale;

    [Tooltip("the tuning for each jump, sequentially")]
    public JumpTuning[] Jumps;

    [Serializable]
    public class JumpTuning {
        [Tooltip("the number of times this jump can be used; 0 = infinite, -1 = none")]
        public int Count = 1;

        [Tooltip("the jump squat duration range")]
        public FloatRange JumpSquatDuration;

        [Tooltip("how long after jump until the character can jump again as a fn of charge percent")]
        public MapOutCurve CooldownDuration;

        [Tooltip("the jump speed a as a fn of charge percent")]
        public MapOutCurve Vertical_Speed;

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
    [Tooltip("the scaling factor of the transfer as a fn of surface angle")]
    public MapOutCurve Surface_AngleScale;

    [FormerlySerializedAs("WallTransferScale")]
    [FormerlySerializedAs("Surface_TransferAttack")]
    [Tooltip("the scaling factor of the transfer as a fn of surface angle change")]
    public MapOutCurve Surface_DeltaScale;

    [Tooltip("the scaling factor of the transfer as a fn of surface angle (when landing)")]
    public MapOutCurve Surface_LandingScale;

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

    // -- friction --
    [Header("friction")]
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

    // -- idle --
    [Header("idle")]
    [Tooltip("the speed threshold under which the character is considered idle (squared)")]
    public float Idle_SqrSpeedThreshold;

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
            CoyoteDuration = 0f;
        } else {
            // make sure coyote time is not greater than jumpsquat duration
            CoyoteDuration = Math.Max(CoyoteDuration, Jumps[0].JumpSquatDuration.Min);
        }
    }
}

}