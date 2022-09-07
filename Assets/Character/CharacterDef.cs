using UnityEngine;

/// the definition for a character
[CreateAssetMenu(menuName = "Character/Def", fileName = "Character")]
sealed class CharacterDef: ScriptableObject {
    // -- fields --
    [Header("fields")]
    [Tooltip("the character prefab")]
    [SerializeField] DisconeCharacter m_Character;

    [Tooltip("the character flower prefab")]
    [SerializeField] CharacterFlower m_Flower;

    // -- queries --
    // the character prefab
    public DisconeCharacter Character {
        get => m_Character;
    }

    // the character flower prefab
    public CharacterFlower Flower {
        get => m_Flower;
    }
}