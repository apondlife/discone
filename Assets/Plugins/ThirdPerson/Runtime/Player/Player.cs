using UnityEngine;
using UnityEngine.Events;

namespace ThirdPerson {

/// the player
public class Player: Player<CharacterInputFrame.Default> {
}

/// the player
public class Player<InputFrame>: MonoBehaviour, PlayerContainer
    where InputFrame: CharacterInputFrame {

    // -- state --
    [Header("state")]
    [Tooltip("the currently controlled character")]
    [SerializeField] Character<InputFrame> m_Character;

    // -- refs --
    [Header("refs")]
    [Tooltip("the player input source")]
    [SerializeField] PlayerInputSource<InputFrame> m_InputSource;

    // -- events --
    [Header("events")]
    [Tooltip("when the player starts driving a character")]
    [SerializeField] UnityEvent<Character<InputFrame>> m_OnDriveStart;

    [Tooltip("when the player stops driving a character")]
    [SerializeField] UnityEvent<Character<InputFrame>> m_OnDriveStop;

    // -- lifecycle --
    void Start() {
        if (m_Character) {
            Drive(m_Character);
        }
    }

    void Update() {
        if (m_Character) {
            SyncCharacter();
        }
    }

    // -- commands --
    /// drive a particular character
    public void Drive(GameObject target) {
        var character = target.GetComponent<Character<InputFrame>>();
        if (character) {
            Drive(character);
        }
    }

    /// drive a particular character
    public void Drive(Character<InputFrame> character) {
        var src = m_Character;
        if (src) {
            src.Drive(null);
            m_OnDriveStop?.Invoke(src);
        }

        var dst = character;
        if (dst) {
            dst.Drive(m_InputSource);
            m_OnDriveStart?.Invoke(dst);
        }

        // set current character and ensure initial state is correct
        m_Character = dst;
        SyncCharacter();
    }

    /// sync state with character
    public void SyncCharacter() {
        transform.position = m_Character.transform.position;
    }

    // -- queries --
    /// the character the player is currently driving
    public Character<InputFrame> Character {
        get => m_Character;
    }

    /// the character the player is currently driving
    public PlayerInputSource<InputFrame> InputSource {
        get => m_InputSource;
    }

    // -- PlayerContainer --
    public Camera Camera {
        get => m_Character.Camera;
    }
}

}