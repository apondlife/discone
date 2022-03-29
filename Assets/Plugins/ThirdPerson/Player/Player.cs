using System;
using UnityEngine;
using UnityEngine.Events;

namespace ThirdPerson {

/// the player
public class Player: MonoBehaviour {
    // -- state --
    [Header("state")]
    [Tooltip("the currently controlled character")]
    [SerializeField] private Character m_CurrentCharacter;

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
            transform.position = m_CurrentCharacter.transform.position;
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
        if(src != null) {
            src.Drive(null);
            src.GetComponentInChildren<ThirdPersonCamera>(true)?.gameObject.SetActive(false);

            m_OnDriveStop?.Invoke(src);
        }

        var dst = character;
        if (dst != null) {
            dst.Drive(m_InputSource);
            dst.GetComponentInChildren<ThirdPersonCamera>(true)?.gameObject.SetActive(true);

            m_OnDriveStart?.Invoke(dst);
        }

        m_CurrentCharacter = dst;
    }

    // -- queries --
    /// the character the player is currently driving
    public Character CurrentCharacter {
        get => m_CurrentCharacter;
    }
}

}