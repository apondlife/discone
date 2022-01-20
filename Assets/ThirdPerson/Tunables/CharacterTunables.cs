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
    [Tooltip("the acceleration from 0 to max speed in units")]
    [SerializeField] private float m_Acceleration;
    public override float Acceleration => m_Acceleration;

    /// the deceleration from 0 to max speed in units
    [Tooltip("the time to stop from max speed")]
    [SerializeField] private float m_Deceleration;
    public override float Deceleration => m_Deceleration;

    [Tooltip("the planar speed when the character stops w/o input")]
    [SerializeField] float m_MinPlanarSpeed;
    public override float MinPlanarSpeed => m_MinPlanarSpeed;

    /// the max speed on the xz plane
    public override float MaxPlanarSpeed => Acceleration / Deceleration;

    /// the time to to reach max speed from zero.
    public override float TimeToMaxSpeed => TimeToPercentMaxSpeed(0.999f);

    /// the time to stop from max speed
    public override float TimeToStop => TimeToPercentMaxSpeed(0.001f);

    [UnityEngine.Serialization.FormerlySerializedAs("_turnSpeed")]
    [Tooltip("the turn speed in radians")]
    [SerializeField] private float m_TurnSpeed;
    public override float TurnSpeed => m_TurnSpeed;

    [UnityEngine.Serialization.FormerlySerializedAs("_pivotSpeed")]
    [Tooltip("the pivot speed in radians")]
    [SerializeField] private float m_PivotSpeed;
    public override float PivotSpeed => m_PivotSpeed;

    [UnityEngine.Serialization.FormerlySerializedAs("_timeToPivot")]
    [Tooltip("the time to finish the pivot deceleration from max speed")]
    [SerializeField] private float m_TimeToPivot;
    public override float TimeToPivot => m_TimeToPivot;

    [UnityEngine.Serialization.FormerlySerializedAs("_pivotStartThreshold")]
    [Tooltip("the pivot start threshold, facing • input dir (-1.0, 1.0f)")]
    [SerializeField] private float m_PivotStartThreshold;
    public override float PivotStartThreshold => m_PivotStartThreshold;

    /// the deceleration of the character while pivoting
    public override float PivotDeceleration => MaxPlanarSpeed / TimeToPivot;

    [Tooltip("the friciton multiplier when turning (max on 90deg turn) ")]
    [SerializeField] private float m_TurningFriction;
    public override float TurningFriction => m_TurningFriction;

    [UnityEngine.Serialization.FormerlySerializedAs("_floatAcceleration")]
    [Tooltip("the planar acceleration while floating")]
    [SerializeField] private float m_FloatAcceleration;
    public override float FloatAcceleration => m_FloatAcceleration;

    #endregion

    #region jump system
    [Header("jump system")]

    [UnityEngine.Serialization.FormerlySerializedAs("_gravity")]
    [Tooltip("the acceleration due to gravity")]
    [SerializeField] private float m_Gravity;
    public override float Gravity => m_Gravity;

    [Tooltip("the min number of frames jump squat lasts")]
    [SerializeField] private int m_MinJumpSquatFrames;
    public override int MinJumpSquatFrames => m_MinJumpSquatFrames;

    [Tooltip("the max number of frames jump squat lasts")]
    [SerializeField] private int m_MaxJumpSquatFrames;
    public override int MaxJumpSquatFrames => m_MaxJumpSquatFrames;

    [UnityEngine.Serialization.FormerlySerializedAs("_jumpSpeedCurve")]
    [Tooltip("how the jump speed changes from holding the squat")]
    [SerializeField] private AnimationCurve m_JumpSpeedCurve;
    public override AnimationCurve JumpSpeedCurve => m_JumpSpeedCurve;

    [Tooltip("the minimum jump speed (minimum length jump squat)")]
    [SerializeField] private float m_MinJumpSpeed;
    public override float MinJumpSpeed => m_MinJumpSpeed;

    /// the minimum jump speed (1-frame jump)
    public override float MinJumpHeight {
        get => MinJumpSpeed * MinJumpSpeed / -(2.0f * Gravity);
    }

    [Tooltip("the maximum jump speed (maximum length jump squat)")]
    [SerializeField] private float m_MaxJumpSpeed;
    public override float MaxJumpSpeed => m_MaxJumpSpeed;

    /// the maximum jump speed (hold jump for duration)
    public override float MaxJumpHeight {
        get => MinJumpSpeed * MaxJumpSpeed / -(2.0f * (Gravity + JumpAcceleration));
    }

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
    #endregion

    #region model / animation
    [Header("model / animation")]

    [UnityEngine.Serialization.FormerlySerializedAs("_tiltForBaseAcceleration")]
    [Tooltip("the angle in degrees character model tilts forward on the start up acceleration")]
    [SerializeField] private float m_TiltForBaseAcceleration;
    public override float TiltForBaseAcceleration => m_TiltForBaseAcceleration;

    [UnityEngine.Serialization.FormerlySerializedAs("_maxTilt")]
    [Tooltip("the maximum angle in degrees the character can tilt")]
    [SerializeField] private float m_MaxTilt;
    public override float MaxTilt => m_MaxTilt;

    [UnityEngine.Serialization.FormerlySerializedAs("_tiltSmoothing")]
    [Tooltip("the smoothing on the character tilt")]
    [SerializeField] private float m_TiltSmoothing;
    public override float TiltSmoothing => m_TiltSmoothing;
    #endregion

    #region camera
    [Header("camera")]

    [UnityEngine.Serialization.FormerlySerializedAs("_dutchScale")]
    [Tooltip("the camera dutch angle (around z-axis) scale applied to the camera's target's rotation")]
    [SerializeField] private float m_DutchScale;
    public override float DutchScale => m_DutchScale;

    [UnityEngine.Serialization.FormerlySerializedAs("_dutchSmoothing")]
    [Tooltip("the smoothing on the camera dutch angle (around z-axis)")]
    [SerializeField] private float m_DutchSmoothing;
    public override float DutchSmoothing => m_DutchSmoothing;
    #endregion

    // -- queries --
    public float TimeToPercentMaxSpeed(float pct) {
        return -Mathf.Log(1.0f - pct, (float)System.Math.E) / Deceleration;
    }
}

}