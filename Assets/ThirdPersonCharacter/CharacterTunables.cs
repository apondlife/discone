using UnityEngine;

[CreateAssetMenu(fileName = "CharacterTunables", menuName = "thirdperson/CharacterTunables", order = 0)]
public class CharacterTunables: CharacterTunablesBase {
    [SerializeField] private float _planarSpeed;
    public override float PlanarSpeed => _planarSpeed;
}
