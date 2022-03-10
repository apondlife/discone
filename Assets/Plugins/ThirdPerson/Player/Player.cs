using UnityEngine;

namespace ThirdPerson {

/// the player
public class Player: MonoBehaviour {
    // -- fields --
    [Tooltip("the player input source")]
    [SerializeField] PlayerInputSource m_InputSource;

    [Tooltip("the character the player is controlling")]
    [SerializeField] private ThirdPerson m_CurrentCharacter;

    void Awake() {
    }

    // -- lifecycle --
    void Start() {
        if (m_CurrentCharacter != null) {
            Drive(m_CurrentCharacter);
        }
    }

    void Update() {
        if (m_CurrentCharacter != null) {
            transform.position = m_CurrentCharacter.transform.position;
        }
    }

    // -- commands --
    /// drive a particular character
    public void Drive(GameObject target) {
        var character = target.GetComponent<ThirdPerson>();
        if (character != null) {
            Drive(character);
        }
    }

    /// drive a particular character
    public void Drive(ThirdPerson character) {
        if(m_CurrentCharacter != null) {
            m_CurrentCharacter.Input.Drive(null);
            // TODO: unity event
            m_CurrentCharacter.GetComponentInChildren<ThirdPersonCamera>(true)?.gameObject.SetActive(false);
            m_CurrentCharacter.GetComponentInChildren<SphereCollider>(true)?.gameObject.SetActive(false);
            m_CurrentCharacter.GetComponentInChildren<BoxCollider>(true)?.gameObject.SetActive(true);
        }

        if (character != null) {
            character.Input.Drive(m_InputSource);
            character.GetComponentInChildren<ThirdPersonCamera>(true)?.gameObject.SetActive(true);
            character.GetComponentInChildren<SphereCollider>(true)?.gameObject.SetActive(true);
            character.GetComponentInChildren<BoxCollider>(true)?.gameObject.SetActive(false);
        }

        m_CurrentCharacter = character;
    }

    // -- queries --
    /// the character the player is currently driving
    public ThirdPerson CurrentCharacter {
        get => m_CurrentCharacter;
    }
}

}