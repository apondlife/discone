using UnityEngine;

namespace ThirdPerson {

[CreateAssetMenu(fileName = "CharacterTunables", menuName = "thirdperson/CharacterTunables", order = 0)]
public sealed class CharacterTunables: CharacterTunablesBase {
    [Header("metadata")]
    [Tooltip("a friendly description for this config")]
    [TextArea(3, 6)]
    [SerializeField] private string m_Description;

    #region movement system
    [Header("movement system")]
    [Tooltip("the horizontal speed at which the character stops")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_MinPlanarSpeed")]
    [SerializeField] float m_Horizontal_MinSpeed;
    public override float Horizontal_MinSpeed => m_Horizontal_MinSpeed;

    /// the character's theoretical max horizontal speed
    public override float Horizontal_MaxSpeed
        => Mathf.Sqrt((Horizontal_Acceleration - Horizontal_KineticFriction) / Horizontal_Drag);

    [Tooltip("the acceleration from 0 to max speed in units")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_Acceleration")]
    [SerializeField] private float m_Horizontal_Acceleration;
    public override float Horizontal_Acceleration => m_Horizontal_Acceleration;

    /// the deceleration from 0 to max speed in units
    [Tooltip("the time to stop from max speed")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_Deceleration")]
    [SerializeField] private float m_Horizontal_Drag;
    public override float Horizontal_Drag => m_Horizontal_Drag;

    [Tooltip("the coefficient of friction when not moving")]
    [SerializeField] private float m_Horizontal_StaticFriction;
    public override float Horizontal_StaticFriction => m_Horizontal_StaticFriction;

    [Tooltip("the coefficient of friction when moving")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_TurningFriction")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_Horizontal_Friction")]
    [SerializeField] private float m_Horizontal_KineticFriction;
    public override float Horizontal_KineticFriction => m_Horizontal_KineticFriction;

    /// the time to to reach max speed from zero.
    public override float TimeToMaxSpeed => TimeToPercentMaxSpeed(
        (Horizontal_MaxSpeed - Horizontal_MinSpeed) / Horizontal_MaxSpeed
    );

    /// the time to stop from max speed
    public override float TimeToStop => TimeToPercentMaxSpeed(
        Horizontal_MinSpeed / Horizontal_MaxSpeed
    );

    [Tooltip("the turn speed in radians")]
    [SerializeField] private float m_TurnSpeed;
    public override float TurnSpeed => m_TurnSpeed;

    [Tooltip("the pivot speed in radians")]
    [SerializeField] private float m_PivotSpeed;
    public override float PivotSpeed => m_PivotSpeed;

    [Tooltip("the time to finish the pivot deceleration from max speed")]
    [SerializeField] private float m_TimeToPivot;
    public override float TimeToPivot => m_TimeToPivot;

    [Tooltip("the pivot start threshold, facing • input dir (-1.0, 1.0f)")]
    [SerializeField] private float m_PivotStartThreshold;
    public override float PivotStartThreshold => m_PivotStartThreshold;

    /// the deceleration of the character while pivoting
    public override float PivotDeceleration => Horizontal_MaxSpeed / TimeToPivot;

    [Tooltip("the planar acceleration while floating")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_FloatAcceleration")]
    [SerializeField] private float m_AerialDriftAcceleration;
    public override float AerialDriftAcceleration => m_AerialDriftAcceleration;

    #endregion

    #region jump system
    [Header("jump system")]
    [Tooltip("the acceleration due to gravity")]
    [SerializeField] private float m_Gravity;
    public override float Gravity => m_Gravity;

    [Tooltip("how many frames you can have pressed jump before landing to execute the jump")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_JumpBuffer")]
    [SerializeField] private uint m_JumpBuffer;
    public override uint JumpBuffer => m_JumpBuffer;

    [Tooltip("max number of frames the character can be in the air and still jump")]
    [SerializeField] private uint m_MaxCoyoteFrames;
    public override uint MaxCoyoteFrames => m_MaxCoyoteFrames;

    [Tooltip("the gravity while holding jump and moving up")]
    [SerializeField] private float m_JumpGravity;
    public override float JumpGravity => m_JumpGravity;

    /// the vertical acceleration while holding jump and moving up
    public override float JumpAcceleration {
        get => m_JumpGravity - m_Gravity;
    }

    [Tooltip("the gravity while holding jump and falling")]
    [SerializeField] private float m_FallGravity;
    public override float FallGravity => m_FallGravity;

    /// the vertical acceleration while holding jump and falling
    public override float FallAcceleration {
        get => m_FallGravity - m_Gravity;
    }

    [Tooltip("the tunables for each jump, sequentially")]
    [SerializeField] private JumpTunables[] m_Jumps;
    public override JumpTunablesBase[] Jumps => m_Jumps;

    [System.Serializable]
    public class JumpTunables : JumpTunablesBase {
        [Tooltip("the number of times this jump can be executed; 0 = infinite")]
        [SerializeField] private uint m_Count = 1;
        public override uint Count => m_Count;

        [Tooltip("how long after this jump the character can jump again")]
        [SerializeField] private uint m_CooldownFrames;
        public override uint CooldownFrames => m_CooldownFrames;

        [Tooltip("the min number of frames jump squat lasts")]
        [SerializeField] private uint m_MinJumpSquatFrames = 5;
        public override uint MinJumpSquatFrames => m_MinJumpSquatFrames;

        [Tooltip("the max number of frames jump squat lasts")]
        [SerializeField] private uint m_MaxJumpSquatFrames = 5;
        public override uint MaxJumpSquatFrames => m_MaxJumpSquatFrames;

        [Tooltip("the minimum jump speed (minimum length jump squat)")]
        [SerializeField] private float m_Vertical_MinSpeed;
        public override float Vertical_MinSpeed => m_Vertical_MinSpeed;

        [Tooltip("the maximum jump speed (maximum length jump squat)")]
        [SerializeField] private float m_Vertical_MaxSpeed;
        public override float Vertical_MaxSpeed => m_Vertical_MaxSpeed;

        [Tooltip("how the jump speed changes from holding the squat")]
        [SerializeField] private AnimationCurve m_Vertical_SpeedCurve;
        public override AnimationCurve Vertical_SpeedCurve => m_Vertical_SpeedCurve;

        [Tooltip("how much upwards speed is cancelled on jump")]
        [SerializeField] private float m_Upwards_MomentumLoss;
        public override float Upwards_MomentumLoss => m_Upwards_MomentumLoss;

        [Tooltip("the minimum horizontal jump speed (minimum length jump squat)")]
        [SerializeField] private float m_Horizontal_MinSpeed;
        public override float Horizontal_MinSpeed => m_Horizontal_MinSpeed;

        [Tooltip("the maximum horizontal jump speed (maximum length jump squat)")]
        [SerializeField] private float m_Horizontal_MaxSpeed;
        public override float Horizontal_MaxSpeed => m_Horizontal_MaxSpeed;

        [Tooltip("how the jump speed changes from holding the squat")]
        [SerializeField] private AnimationCurve m_Horizontal_SpeedCurve;
        public override AnimationCurve Horizontal_SpeedCurve => m_Horizontal_SpeedCurve;

        [Tooltip("how much vertical speed is cancelled on jump")]
        [SerializeField] private float m_Horizontal_MomentumLoss;
        public override float Horizontal_MomentumLoss => m_Horizontal_MomentumLoss;
    }

    #endregion

    #region wall
    [Header("wall")]

    [Tooltip("the collision layer of what counts as walls for wall sliding")]
    [SerializeField] private LayerMask m_WallLayer;
    public override LayerMask WallLayer => m_WallLayer;

    [Tooltip("the gravity while holding jump and walling")]
    [SerializeField] private float m_WallGravity;
    public override float WallGravity => m_WallGravity;

    /// the vertical acceleration while holding jump and Walling
    public override float WallAcceleration {
        get => m_WallGravity - m_Gravity + m_FallGravity;
    }

    [Tooltip("the force the wall pull the character to make it stick")]
    [SerializeField] public float m_WallMagnet;
    public override float WallMagnet => m_WallMagnet;
    #endregion

    #region model / animation
    [Header("model / animation")]

    [Tooltip("the angle in degrees character model tilts forward on the start up acceleration")]
    [SerializeField] private float m_TiltForBaseAcceleration;
    public override float TiltForBaseAcceleration => m_TiltForBaseAcceleration;

    [Tooltip("the maximum angle in degrees the character can tilt")]
    [SerializeField] private float m_MaxTilt;
    public override float MaxTilt => m_MaxTilt;

    [Tooltip("the smoothing on the character tilt")]
    [SerializeField] private float m_TiltSmoothing;
    public override float TiltSmoothing => m_TiltSmoothing;
    #endregion

    #region camera
    [Header("camera")]
    [Tooltip("the camera dutch angle (around z-axis) scale applied to the camera's target's rotation")]
    [SerializeField] private float m_DutchScale;
    public override float DutchScale => m_DutchScale;

    [Tooltip("the smoothing on the camera dutch angle (around z-axis)")]
    [SerializeField] private float m_DutchSmoothing;
    public override float DutchSmoothing => m_DutchSmoothing;

    [Tooltip("the default da")]
    [Range(0.0f, 20.0f)]
    [SerializeField] private float m_Damping;
    public override float Damping => m_Damping;

    [Tooltip("the yaw damping when holding the recenter button")]
    [Range(0.0f, 20.0f)]
    [SerializeField] private float m_FastDamping;
    public override float FastDamping => m_FastDamping;

    #endregion

    // -- queries --
    public float TimeToPercentMaxSpeed(float pct) {
        return -Mathf.Log(1.0f - pct, (float)System.Math.E) / Horizontal_Drag;
    }

    public void OnValidate() {
        if (m_Jumps.Length == 0) {
            m_MaxCoyoteFrames = 0;
        } else {
            m_MaxCoyoteFrames = System.Math.Max(m_MaxCoyoteFrames, m_Jumps[0].MinJumpSquatFrames);
        }
    }
}

}