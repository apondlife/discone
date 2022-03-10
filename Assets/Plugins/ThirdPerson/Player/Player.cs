using System;
using UnityEngine;
using UnityEngine.Events;

namespace ThirdPerson {

/// the player
public class Player: MonoBehaviour {
    // -- state --
    [Header("fields")]
    [Tooltip("the currently controlled character")]
    [SerializeField] private ThirdPerson m_CurrentCharacter;

    // -- references --
    [Header("references")]
    [Tooltip("the player input source")]
    [SerializeField] PlayerInputSource m_InputSource;

    // -- events --
    [Header("events")]
    [Tooltip("when the player starts driving a character")]
    [SerializeField] UnityEvent<ThirdPerson> m_OnDriveStart;

    [Tooltip("when the player stops driving a character")]
    [SerializeField] UnityEvent<ThirdPerson> m_OnDriveStop;

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
        var src = m_CurrentCharacter;
        if(src != null) {
            src.Input.Drive(null);
            m_OnDriveStart?.Invoke(src);

            src.GetComponentInChildren<ThirdPersonCamera>(true)?.gameObject.SetActive(false);
            src.GetComponentInChildren<SphereCollider>(true)?.gameObject.SetActive(false);
            src.GetComponentInChildren<BoxCollider>(true)?.gameObject.SetActive(true);
        }

        var dst = character;
        if (dst != null) {
            dst.Input.Drive(m_InputSource);
            m_OnDriveStop?.Invoke(src);

            dst.GetComponentInChildren<ThirdPersonCamera>(true)?.gameObject.SetActive(true);
            dst.GetComponentInChildren<SphereCollider>(true)?.gameObject.SetActive(true);
            dst.GetComponentInChildren<BoxCollider>(true)?.gameObject.SetActive(false);
        }

        m_CurrentCharacter = dst;
    }

    // -- queries --
    /// the character the player is currently driving
    public ThirdPerson CurrentCharacter {
        get => m_CurrentCharacter;
    }
}

}