using UnityEngine;
using ThirdPerson;
using UnityAtoms;

/// <summary>
/// Event of type `ThirdPerson.Character`. Inherits from `AtomEvent&lt;Character&gt;`.
/// </summary>
[EditorIcon("atom-icon-cherry")]
[CreateAssetMenu(menuName = "Unity Atoms/Events/Character", fileName = "CharacterEvent")]
public sealed class CharacterEvent: AtomEvent<Character> {
}