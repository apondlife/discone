using UnityEngine;

using ThirdPerson;

[CreateAssetMenu(fileName = "DisconeCharacter", menuName = "discone/DisconeCharacter", order = 0)]
public class DisconeCharacter : ScriptableObject {
    public string Name;
    public string YarnNode;
    public CharacterModel PlayablePrefab;
    public CharacterTunables Tunables;
}