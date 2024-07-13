using UnityEngine;

namespace Discone {
    /// the definition for a character
    [CreateAssetMenu(menuName = "Character/Def", fileName = "Character")]
    sealed class CharacterDef : ScriptableObject {
        // -- fields --
        [Header("fields")]
        [Tooltip("the character prefab")]
        [SerializeField] Character m_Character;

        [Tooltip("the character flower prefab")]
        [SerializeField] Flower m_Flower;

        // -- placeholder --
        [Header("placeholder")]
        [Tooltip("the placeholder prefab")]
        [SerializeField] GameObject m_Placeholder;

        [Tooltip("the placeholder offset")]
        [SerializeField] Vector3 m_Placeholder_Offset;

        // -- queries --
        // the character prefab
        public Character Character {
            get => m_Character;
        }

        // the character flower prefab
        public Flower Flower {
            get => m_Flower;
        }

        // the character placeholder prefab
        public GameObject Placeholder {
            get => m_Placeholder;
        }

        // the character placeholder prefab position offset
        public Vector3 Placeholder_Offset {
            get => m_Placeholder_Offset;
        }
    }
}