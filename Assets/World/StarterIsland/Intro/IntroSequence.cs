using System.Threading;
using ThirdPerson;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Discone {

public class IntroSequence: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the delay before the intro is active")]
    [SerializeField] EaseTimer m_Delay;

    // -- refs --
    [Header("refs")]
    [Tooltip("the current character")]
    [SerializeField] DisconeCharacterVariable m_CurrentCharacter;

    [Tooltip("when the intro is over")]
    [SerializeField] VoidEvent m_IntroEnded;

    [Tooltip("the intro camera")]
    [SerializeField] GameObject m_IntroCamera;

    [Tooltip("the shared data store")]
    [SerializeField] Store m_Store;

    // -- props --
    /// the set of event subscriptions
    DisposeBag m_Subscriptions = new DisposeBag();

    // -- lifecycle --
    void Start() {
        m_Subscriptions.Add(m_Store.LoadFinished, OnLoadFinished);
    }

    void FixedUpdate() {
        m_Delay.Tick();

        if (m_Delay.Raw >= 1f && !m_CurrentCharacter.Value.Character.State.IsIdle) {
            Finish();
        }
    }

    // -- commands --
    /// finish the sequnce and destroy it
    void Finish() {
        m_IntroEnded.Raise();
        m_IntroCamera.SetActive(false);
        Destroy(this);
    }

    // -- events --
    void OnLoadFinished() {
        if (m_Store.Player.HasData) {
            Finish();
        } else {
            m_Delay.Start();
            m_IntroCamera.SetActive(true);
        }
    }
}

}