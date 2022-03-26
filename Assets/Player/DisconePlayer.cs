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
        m_DriveStart.Register(OnDriveStop);
        m_IsDialogueActiveChanged.Register(OnIsDialogueActiveChanged);
    }

    // -- events --
    // when the player starts driving a character
    void OnDriveStart(Character character) {
        var dialogue = character.GetComponent<NPCDialogue>();
        Debug.Assert(dialogue != null, $"character {character.name} has no dialogue attached.");
        dialogue?.StopListening();
    }

    // when the player stops driving a character
    void OnDriveStop(Character character) {
        var dialogue = character.GetComponent<NPCDialogue>();
        Debug.Assert(dialogue != null, $"character {character.name} has no dialogue attached.");
        dialogue?.StartListening();
    }

    /// when the dialog becomes in/active
    void OnIsDialogueActiveChanged(bool isDialogueActive) {
        m_InputSource.enabled = !isDialogueActive;
    }
}