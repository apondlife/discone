using UnityEngine;
using UnityAtoms.BaseAtoms;
using ThirdPerson;

/// the discone player
sealed class DisconePlayer: MonoBehaviour {
    // -- events --
    [Header("events")]
    [Tooltip("when the player starts driving a character")]
    [SerializeField] CharacterEvent m_DriveStart;

    [Tooltip("when the player stops driving a character")]
    [SerializeField] CharacterEvent m_DriveStop;

    [Tooltip("if the dialogue is active")]
    [SerializeField] BoolEvent m_IsDialogueActiveChanged;

    // -- refs --
    [Header("refs")]
    [Tooltip("the input source")]
    [SerializeField] PlayerInputSource m_InputSource;

    // -- lifecycle --
    void Awake() {
        // bind events
        m_DriveStart.Register(OnDriveStart);
        m_DriveStop.Register(OnDriveStop);
        m_IsDialogueActiveChanged.Register(OnIsDialogueActiveChanged);
    }

    // -- queries --
    /// get the dialogue attached to a character
    NPCDialogue FindDialogue(Character character) {
        var dialogue = character.GetComponentInChildren<NPCDialogue>();
        Debug.Assert(dialogue != null, $"character {character.name} has no dialogue attached.");
        return dialogue;
    }

    // -- events --
    // when the player starts driving a character
    void OnDriveStart(Character character) {
        FindDialogue(character)?.StopListening();
    }

    // when the player stops driving a character
    void OnDriveStop(Character character) {
        FindDialogue(character)?.StartListening();
    }

    /// when the dialog becomes in/active
    void OnIsDialogueActiveChanged(bool isDialogueActive) {
        m_InputSource.enabled = !isDialogueActive;
    }
}