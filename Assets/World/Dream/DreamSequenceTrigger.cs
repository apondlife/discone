using System;
using System.Runtime.Remoting.Services;
using NaughtyAttributes;
using ThirdPerson;
using UnityEngine;

namespace Discone {

[RequireComponent(typeof(Collider))]
sealed class DreamSequenceTrigger: MonoBehaviour {
    enum Event {
        Enter,
        Stay,
        Exit,
    }

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the kind of event to trigger")]
    [SerializeField] Event m_Event;

    [Tooltip("the duration for the stay event to fire")]
    [ShowIf(nameof(m_Event), Event.Stay)]
    [SerializeField] float m_StayDuration;

    // -- props --
    /// an action to to fire on trigger enter
    Action m_OnFire;

    /// an action to to fire on trigger enter
    float m_StayElapsed;

    // -- commands --
    /// enables/disables the trigger
    public void Toggle(bool isEnabled) {
        gameObject.SetActive(isEnabled);
    }

    /// finish the step associated with this trigger
    public void Finish() {
        Destroy(gameObject);
    }

    /// fires the trigger's action
    void Fire() {
        m_OnFire?.Invoke();
    }

    /// add a listener to the trigger's enter event
    public void OnFire(Action action) {
        m_OnFire += action;
    }

    // -- queries --
    /// if the collider is the current player
    static bool IsPlayer(Collider other) {
        var player = other.GetComponentInParent<Player>();
        return player && player.Character;
    }

    // -- events --
    void OnTriggerEnter(Collider other) {
        if (!IsPlayer(other)) {
            return;
        }

        if (m_Event != Event.Enter) {
            return;
        }

        Fire();
    }

    void OnTriggerStay(Collider other) {
        if (!IsPlayer(other)) {
            return;
        }

        if (m_Event != Event.Stay) {
            return;
        }

        m_StayElapsed += Time.deltaTime;
        if (m_StayElapsed >= m_StayDuration) {
            Fire();
        }
    }

    void OnTriggerExit(Collider other) {
        if (!IsPlayer(other)) {
            return;
        }

        if (m_Event == Event.Stay) {
            m_StayElapsed = 0f;
        }

        if (m_Event != Event.Exit) {
            return;
        }

        Fire();
    }
}

}