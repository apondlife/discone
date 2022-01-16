using UnityEngine;

[CreateAssetMenu(fileName = "CharacterTunables", menuName = "thirdperson/CharacterTunables", order = 0)]
public class CharacterTunables : CharacterTunablesBase {
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

    [Tooltip("the acceleration due to gravity")]
    [SerializeField] private float _gravity;
    public override float Gravity => _gravity;

    [Tooltip("the acceleration due to gravity")]
    [SerializeField] private float _initialJumpSpeed;
    public override float InitialJumpSpeed => _initialJumpSpeed;

    [Header("jump system")]
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

    [Tooltip("the acceleration while holding jump and airborne")]
    [SerializeField] private float _floatAcceleration;
    public override float FloatAcceleration => _floatAcceleration;

    // -- queries --
    /// the acceleration from 0 to max speed in units
    public override float Acceleration => MaxPlanarSpeed / TimeToMaxSpeed;

    /// the deceleration from 0 to max speed in units
    public override float Deceleration => MaxPlanarSpeed / TimeToStop;

    /// the deceleration of the character while pivoting
    public override float PivotDeceleration => MaxPlanarSpeed / TimeToPivot;
}
