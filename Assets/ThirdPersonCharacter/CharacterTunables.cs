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

    [Tooltip("the acceleration due to gravity")]
    [SerializeField] private float _gravity;
    public override float Gravity => _gravity;

    [Tooltip("the acceleration due to gravity")]
    [SerializeField] private float _initialJumpSpeed;
    public override float InitialJumpSpeed => _initialJumpSpeed;

    // -- queries --
    /// the acceleration from 0 to max speed in units
    public override float Acceleration => MaxPlanarSpeed / TimeToMaxSpeed;

    /// the deceleration from 0 to max speed in units
    public override float Deceleration => MaxPlanarSpeed / TimeToStop;
}
