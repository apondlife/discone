using UnityEngine;

[CreateAssetMenu(fileName = "CharacterTunables", menuName = "thirdperson/CharacterTunables", order = 0)]
public class CharacterTunables: CharacterTunablesBase {
    #region movement system
    [Header("movement system")]

    [Tooltip("the max speed on the xz plane")]
    [SerializeField] private float _maxPlanarSpeed;
    public override float MaxPlanarSpeed => _maxPlanarSpeed;

    [Tooltip("the time to to reach max speed from zero.")]
    [SerializeField] private float _timeToMaxSpeed;
    public override float TimeToMaxSpeed => _timeToMaxSpeed;

    [Tooltip("the time to stop from max speed")]
    [SerializeField] private float _timeToStop;
    public override float TimeToStop => _timeToStop;

    [Tooltip("the turn speed in radians")]
    [SerializeField] private float _turnSpeed;
    public override float TurnSpeed => _turnSpeed;

    [Tooltip("the pivot speed in radians")]
    [SerializeField] private float _pivotSpeed;
    public override float PivotSpeed => _pivotSpeed;

    [Tooltip("the time to finish the pivot deceleration from max speed")]
    [SerializeField] private float _timeToPivot;
    public override float TimeToPivot => _timeToPivot;

    [Tooltip("the pivot start threshold, facing • input dir (-1.0, 1.0f)")]
    [SerializeField] private float _pivotStartThreshold;
    public override float PivotStartThreshold => _pivotStartThreshold;

    [Tooltip("the planar acceleration while floating")]
    [SerializeField] private float _floatAcceleration;
    public override float FloatAcceleration => _floatAcceleration;

    #endregion

    #region jump system
    [Header("jump system")]
    [Tooltip("the acceleration due to gravity")]
    [SerializeField] private float _gravity;
    public override float Gravity => _gravity;

    [Tooltip("the acceleration due to gravity")]
    [SerializeField] private float _initialJumpSpeed;
    public override float InitialJumpSpeed => _initialJumpSpeed;

    [Tooltip("the number of frames jump squat lasts")]
    [SerializeField] private int _jumpSquatFrames;
    public override int JumpSquatFrames => _jumpSquatFrames;

    [Tooltip("the minimum jump speed (minimum length jump squat)")]
    [SerializeField] private float _minJumpSpeed;
    public override float MinJumpSpeed => _minJumpSpeed;

    [Tooltip("the maximum jump speed (maximum length jump squat)")]
    [SerializeField] private float _maxJumpSpeed;
    public override float MaxJumpSpeed => _maxJumpSpeed;

    [Tooltip("how the jump speed changes from holding the squat")]
    [SerializeField] private AnimationCurve _jumpSpeedCurve;
    public override AnimationCurve JumpSpeedCurve => _jumpSpeedCurve;

    [Tooltip("the vertical acceleration while holding jump and airborne")]
    [SerializeField] private float _jumpAcceleration;
    public override float JumpAcceleration => _jumpAcceleration;
    #endregion

    #region model / animation
    [Header("model / animation")]

    [Tooltip("the angle in degrees character model tilts forward on the start up acceleration")]
    [SerializeField] private float _tiltForBaseAcceleration;
    public override float TiltForBaseAcceleration => _tiltForBaseAcceleration;

    [Tooltip("the maximum angle in degrees the character can tilt")]
    [SerializeField] private float _maxTilt;
    public override float MaxTilt => _maxTilt;

    [Tooltip("the smoothing on the character tilt")]
    [SerializeField] private float _tiltSmoothing;
    public override float TiltSmoothing => _tiltSmoothing;
    #endregion

    #region camera
    [Header("camera")]

    [Tooltip("the camera dutch angle (around z-axis) scale applied to the camera's target's rotation")]
    [SerializeField] private float _dutchScale;
    public override float DutchScale => _dutchScale;

    [Tooltip("the smoothing on the camera dutch angle (around z-axis)")]
    [SerializeField] private float _dutchSmoothing;
    public override float DutchSmoothing => _dutchSmoothing;
    #endregion

    // -- queries --
    /// the acceleration from 0 to max speed in units
    public override float Acceleration => MaxPlanarSpeed / TimeToMaxSpeed;

    /// the deceleration from 0 to max speed in units
    public override float Deceleration => MaxPlanarSpeed / TimeToStop;

    /// the deceleration of the character while pivoting
    public override float PivotDeceleration => MaxPlanarSpeed / TimeToPivot;

    /// the deceleration of the character while pivoting
    public float JumpHeight =>  InitialJumpSpeed * InitialJumpSpeed / ( -2*Gravity);
    public float JumpDuration =>  -InitialJumpSpeed / Gravity;
    // public float PivotDeceleration => MaxPlanarSpeed / TimeToPivot;
}
