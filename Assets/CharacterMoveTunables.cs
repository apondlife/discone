using UnityEngine;

[CreateAssetMenu(fileName = "CharacterMovementTunables", menuName = "thirdperson/CharacterMoveTunables", order = 0)]
public class CharacterMoveTunables : CharacterMoveTunablesBase
{
    [SerializeField] private float _planarSpeed;
    public override float PlanarSpeed => _planarSpeed;
}
