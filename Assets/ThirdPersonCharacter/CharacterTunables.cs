using UnityEngine;

[CreateAssetMenu(fileName = "CharacterTunables", menuName = "thirdperson/CharacterTunables", order = 0)]
public class CharacterTunables: CharacterTunablesBase {
    [SerializeField] private float _planarSpeed;
    public override float PlanarSpeed => _planarSpeed;

    [SerializeField] private float _turnSpeed;
    public override float TurnSpeed => _turnSpeed;

    [SerializeField] private float _gravity;
    public override float Gravity => _gravity;

    [SerializeField] private float _initialJumpSpeed;
    public override float InitialJumpSpeed => _initialJumpSpeed;
}
