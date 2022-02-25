using UnityEngine;

namespace ThirdPerson {

/// the player
public class Player: MonoBehaviour {
    // -- fields --
    [Tooltip("the player input source")]
    [SerializeField] PlayerInputSource m_InputSource;

    [Tooltip("the character the player is controlling")]
    [SerializeField] private ThirdPerson m_CurrentCharacter;

    // -- lifecycle --
    void Start() {
        Drive(m_CurrentCharacter);
    }

    // -- commands --
    /// drive a particular character
    public void Drive(GameObject target) {
        var character = target.GetComponent<ThirdPerson>();
        if(character != null) {
            Drive(character);
        }
    }

    public void Drive(ThirdPerson character) {
        m_CurrentCharacter.Input.Drive(null);
        m_CurrentCharacter.GetComponentInChildren<ThirdPersonCamera>(true)?.gameObject.SetActive(false);
        m_CurrentCharacter.GetComponentInChildren<SphereCollider>(true)?.gameObject.SetActive(false);
        m_CurrentCharacter.GetComponentInChildren<BoxCollider>(true)?.gameObject.SetActive(true);

        character.Input.Drive(m_InputSource);
        character.GetComponentInChildren<ThirdPersonCamera>(true)?.gameObject.SetActive(true);
        character.GetComponentInChildren<SphereCollider>(true)?.gameObject.SetActive(true);
        character.GetComponentInChildren<BoxCollider>(true)?.gameObject.SetActive(false);

        m_CurrentCharacter = character;
    }
}

}