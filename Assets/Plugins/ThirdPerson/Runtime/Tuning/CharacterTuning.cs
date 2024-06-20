using System;
using Soil;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {

[CreateAssetMenu(fileName = "CharacterTuning", menuName = "thirdperson/CharacterTuning", order = 0)]
public sealed partial class CharacterTuning: ScriptableObject {
    // -- metadata --
    [Foldout("metadata")]
    [Tooltip("a friendly description for this config")]
    [TextArea(3, 6)]
    [SerializeField] string m_Description;

    // -- movement --
    [Foldout("movement")]
    [FormerlySerializedAs("Horizontal_MinSpeed")]
    [Tooltip("the horizontal speed at which the character stops")]
    public float Surface_MinSpeed;

    /// the character's theoretical max surface speed
    public float Surface_MaxSpeed {
        get => Mathf.Sqrt(Mathf.Max(0.0f, Surface_Acceleration.Evaluate(0f) - Friction_Kinetic) / Friction_SurfaceDrag);
    }

    [Tooltip("the movement acceleration as a fn of surface angle")]
    public MapOutCurve Surface_Acceleration;

    [Tooltip("the turn speed in degrees")]
    public float TurnSpeed;

    [Tooltip("the pivot speed in degrees")]
    public float PivotSpeed;

    [Tooltip("the time to finish the pivot deceleration from max speed")]
    public float TimeToPivot;

    [Tooltip("the angle to start pivot, facing • input dir")]
    public float PivotStartThreshold;

    [Tooltip("the angle between pivot dir and facing when pivot ends")]
    public float PivotEndThreshold;

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
    [Foldout("crouch")]
    [Tooltip("the crouch power as a fn of time crouching")]
    public AdsrCurve Crouch_Power;

    [Tooltip("the static friction value when crouching")]
    public float Crouch_StaticFriction;

    [Tooltip("the turn speed while crouching")]
    public float Crouch_TurnSpeed;

    [Tooltip("the inline inpucharacter model when crouching/sliding")]
    public MapOutCurve Crouch_InlineScale;

    [Tooltip("the cross input scale when crouching/sliding")]
    public MapOutCurve Crouch_CrossScale;

    [Tooltip("the gravity while crouching")]
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
    [Foldout("jump")]
    [Tooltip("the acceleration due to gravity")]
    public float Gravity;

    [Tooltip("the acceleration due to gravity when going upwards")]
    public float Gravity_Jump;

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

    [NonReorderable]
    [Tooltip("the tuning for each jump, sequentially")]
    public JumpTuning[] Jumps;

    [Serializable]
    public class JumpTuning {
        [Tooltip("the number of times this jump can be used; 0 = infinite, -1 = none")]
        public int Count = 1;

        [FormerlySerializedAs("m_JumpPower")]
        [FormerlySerializedAs("JumpPower")]
        [Tooltip("the jump power as a fn of jump squat elapsed")]
        [SerializeField] AdsrCurve m_Power;

        [FormerlySerializedAs("Lift_Jump")]
        [FormerlySerializedAs("Acceleration")]
        [Tooltip("the vertical acceleration starting @ jump jump")]
        public AdsrCurve Lift;

        [Tooltip("how long after jump until the character can jump again as a fn of charge percent")]
        public MapOutCurve CooldownDuration;

        [Tooltip("the jump speed a as a fn of charge percent")]
        public MapOutCurve Vertical_Speed;

        [Tooltip("how much upwards speed is cancelled on jump")]
        public float Upwards_MomentumLoss;

        [Tooltip("how much horizontal speed is cancelled on jump")]
        public float Horizontal_MomentumLoss;

        [Header("charge")]
        [FormerlySerializedAs("JumpSquat_ShouldAutoJump")]
        [FormerlySerializedAs("ShouldJumpAfterJumpSquat")]
        [Tooltip("if this jumps happens automatically after the jump squat duration")]
        public bool Charge_ShouldAutoJump;

        [FormerlySerializedAs("JumpSquat_Duration")]
        [FormerlySerializedAs("JumpSquatDuration")]
        [Tooltip("the jump squat charge duration")]
        public FloatRange Charge_Duration;

        [FormerlySerializedAs("Lift_Hold")]
        [Tooltip("the vertical acceleration starting @ jump squat")]
        public float Charge_Lift;

        // -- queries --
        /// the jump power as a fn of jump squat elapsed
        public float Power(float elapsed) {
            return m_Power.Evaluate(elapsed - Charge_Duration.Min);
        }
    }

    // -- queries --
    /// get the jump tuning for the id
    public JumpTuning JumpById(JumpId id) {
        return Jumps[id.Index];
    }

    /// get the next jump tuning (to initiate a jump)
    public JumpTuning NextJump(CharacterState state) {
        return JumpById(state.Next.NextJump);
    }

    // -- surface --
    [Foldout("surface")]
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

    [Tooltip("how much upwards velocity we add to our velocity projection tangent as a fn of surface angle")]
    public MapOutCurve Surface_UpwardsVelocityBias;

    [Tooltip("the time it takes the perceived surface to scale between 0 & 1")]
    public float Surface_PerceptionDuration;

    [Tooltip("the speed the perceived surface's normal moves towards a new normal")]
    public float Surface_PerceptionAngularSpeed;

    // -- friction --
    [Foldout("friction")]
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
    [Foldout("idle")]
    [Tooltip("the speed threshold under which the character is considered idle (squared)")]
    public float Idle_SqrSpeedThreshold;

    // -- lifecycle --
    void OnValidate() {
        if (Jumps == null || Jumps.Length == 0) {
            CoyoteDuration = 0f;
        } else {
            // make sure coyote time is not greater than jumpsquat duration
            CoyoteDuration = Math.Max(CoyoteDuration, Jumps[0].Charge_Duration.Min);
        }
    }
}

}