using UnityEngine;
using UnityEngine.Events;

namespace ThirdPerson {

/// the player
public class Player: MonoBehaviour {
    // -- state --
    [Header("state")]
    [Tooltip("the currently controlled character")]
    [SerializeField] Character m_CurrentCharacter;

    // -- refs --
    [Header("refs")]
    [Tooltip("the player input source")]
    [SerializeField] PlayerInputSource m_InputSource;

    // -- events --
    [Header("events")]
    [Tooltip("when the player starts driving a character")]
    [SerializeField] UnityEvent<Character> m_OnDriveStart;

    [Tooltip("when the player stops driving a character")]
    [SerializeField] UnityEvent<Character> m_OnDriveStop;

    // -- lifecycle --
    void Start() {
        if (m_CurrentCharacter != null) {
            Drive(m_CurrentCharacter);
        }
    }

    void Update() {
        if (m_CurrentCharacter != null) {
            SyncCharacter();
        }
    }

    // -- commands --
    /// drive a particular character
    public void Drive(GameObject target) {
        var character = target.GetComponent<Character>();
        if (character != null) {
            Drive(character);
        }
    }

    /// drive a particular character
    public void Drive(Character character) {
        var src = m_CurrentCharacter;
        if (src != null) {
            src.Drive(null);
            // TODO(fun): i don't like this
            src.GetComponentInChildren<ThirdPerson.Camera>(true)?.gameObject.SetActive(false);

            m_OnDriveStop?.Invoke(src);
        }

        var dst = character;
        if (dst != null) {
            dst.Drive(m_InputSource);
            // TODO(fun): i don't like this
            dst.GetComponentInChildren<ThirdPerson.Camera>(true)?.gameObject.SetActive(true);

            m_OnDriveStart?.Invoke(dst);
        }

        // set current character and ensure initial state is correct
        m_CurrentCharacter = dst;
        SyncCharacter();
    }

    /// sync state with character
    public void SyncCharacter() {
        transform.position = m_CurrentCharacter.transform.position;
    }

    // -- queries --
    /// the character the player is currently driving
    public Character CurrentCharacter {
        get => m_CurrentCharacter;
    }
}

}