using Cinemachine;
using UnityEngine;
using UnityEngine.Events;

namespace ThirdPerson {

/// the player
public class Player: Player<CharacterInputFrame.Default> {
    // -- input --
    [Header("input")]
    [Tooltip("the player's mapped input source")]
    [SerializeField] PlayerInputSource m_InputSource;

    // -- Player<InputFrame> --
    /// the character the player is currently driving
    public override PlayerInputSource<CharacterInputFrame.Default> InputSource {
        get => m_InputSource;
    }
}

/// the player
public abstract class Player<InputFrame>: MonoBehaviour, PlayerContainer
    where InputFrame: CharacterInputFrame {

    // -- cfg --
    [Header("cfg")]
    [Tooltip("if this player toggles its camera on drive")]
    [SerializeField] bool m_IsCameraOwner = true;

    // -- state --
    [Header("state")]
    [Tooltip("the currently controlled character")]
    [SerializeField] Character<InputFrame> m_Character;

    // -- events --
    [Header("events")]
    [Tooltip("when the player starts driving a character")]
    [SerializeField] UnityEvent<Character<InputFrame>> m_OnDriveStart;

    [Tooltip("when the player stops driving a character")]
    [SerializeField] UnityEvent<Character<InputFrame>> m_OnDriveStop;

    // -- lifecycle --
    protected virtual void Awake() {
    }

    protected virtual void Start() {
        if (m_Character) {
            Drive(m_Character);
        }
    }

    protected virtual void Update() {
        if (m_Character) {
            SyncCharacter();
        }
    }

    protected virtual void OnDestroy() {
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
            src.Release();
            if (m_IsCameraOwner) {
                src.Camera.Toggle(false);
            }

            m_OnDriveStop?.Invoke(src);
        }

        var dst = character;
        if (dst) {
            dst.Drive(InputSource);
            if (m_IsCameraOwner) {
                dst.Camera.Toggle(true);
            }

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
    public abstract PlayerInputSource<InputFrame> InputSource {
        get;
    }

    /// the character the player is currently driving
    public Character<InputFrame> Character {
        get => m_Character;
    }

    // -- PlayerContainer --
    public Camera Camera {
        get => m_Character ? m_Character.Camera : null;
    }
}

}