using System;
using UnityAtoms;
using UnityEngine;

namespace Discone {

sealed class GameStepTrigger: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the step for this trigger")]
    [SerializeField] GameStep m_Step;

    [Tooltip("the collider")]
    [SerializeField] Collider m_Collider;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("when a game step starts")]
    [SerializeField] GameStepEvent m_GameStep_Started;

    // -- props --
    /// an action to to fire on trigger enter
    Action<GameStep> m_OnEnter;

    /// an action to to fire on trigger exit
    Action<GameStep> m_OnExit;

    /// a set of event subscriptions
    DisposeBag m_Subscriptions = new();

    // -- lifecycle --
    void Start() {
        m_Subscriptions.Add(m_GameStep_Started, OnGameStepStarted);
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// add a listener to the trigger's enter event
    public void OnEnter(Action<GameStep> action) {
        m_OnEnter += action;
    }

    /// add a listener to the trigger's exit event
    public void OnExit(Action<GameStep> action) {
        m_OnExit += action;
    }

    /// fires the trigger's enter action
    void FireEnter() {
        m_OnEnter?.Invoke(m_Step);
    }

    /// fires the trigger's exit action
    void FireExit() {
        m_OnExit?.Invoke(m_Step);
    }

    // -- events --
    void OnGameStepStarted(GameStep step) {
        if (step > m_Step) {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.IsLocalPlayer()) {
            FireEnter();
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.IsLocalPlayer()) {
            FireExit();
        }
    }
}

}