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

    // -- lifecycle --
    void Start() {
        m_Delay.Start();
    }

    void FixedUpdate() {
        m_Delay.Tick();

        if (m_Delay.Raw >= 1f && !m_CurrentCharacter.Value.Character.State.IsIdle) {
            m_IntroEnded.Raise();
            m_IntroCamera.SetActive(false);
            Destroy(this);
        }
    }
}

}