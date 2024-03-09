using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Discone {

/// talking to the character
public sealed class CharacterDialogue: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("IMPORTANT: the title of the yarn node for this characters's dialogue")]
    [UnityEngine.Serialization.FormerlySerializedAs("dialogueMessage")]
    [SerializeField] string m_NodeTitle;

    [Tooltip("the bouncing talkability indicator")]
    [UnityEngine.Serialization.FormerlySerializedAs("talkable")]
    [SerializeField] CharacterDialogueIndicator m_TalkIndicator;

    // -- published --
    [Header("published")]
    [Tooltip("start the dialogue for this character")]
    [SerializeField] GameObjectEvent m_StartDialogue;

    // -- refs --
    [Header("refs")]
    [Tooltip("the input action")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_Talk")]
    [SerializeField] InputActionReference m_TalkInput;

    [Tooltip("a reference to the current character")]
    [SerializeField] DisconeCharacterVariable m_CurrentCharacter;

    // -- props --
    // the parent character
    DisconeCharacter m_Character;

    /// if this character is currently talkable
    bool m_IsTalkable;

    // -- lifecycle --
    void Start() {
        // get parent
        m_Character = GetComponentInParent<DisconeCharacter>();

        // TODO: do in prefab
        if (m_TalkIndicator) {
            m_TalkIndicator.Hide();
        }
    }

    void OnDestroy() {
        if (m_IsTalkable) {
            SetIsTalkable(false);
        }
    }

    // -- commands --
    /// toggles the character's talkability
    public void SetIsTalkable(bool isTalkable) {
        if (isTalkable == m_IsTalkable) {
            return;
        }

        // update state
        m_IsTalkable = isTalkable;

        // toggle indicator
        m_TalkIndicator.SetIsVisible(isTalkable);

        // toggle input
        // TODO: move this into PlayerDialogue, raise a character event to change
        // dialogue target
        var input = m_TalkInput.action;
        if (isTalkable) {
            input.performed += OnTalkPressed;
        } else {
            input.performed -= OnTalkPressed;
        }
    }

    // -- queries --
    /// the title of the dialogue node to start
    public string NodeTitle {
        get => m_NodeTitle;
    }

    /// the character for this dialogue
    public GameObject Character {
        get => m_Character.gameObject;
    }

    // -- events --
    /// when the player presses talk
    void OnTalkPressed(InputAction.CallbackContext _) {
        /// TODO: raise DisconeCharacter event?
        m_StartDialogue.Raise(gameObject);
    }
}

}