using ThirdPerson;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Discone {

public class IntroSequence: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the delay before the intro is active")]
    [SerializeField] EaseTimer m_Delay;

    // -- inputs --
    [Header("inputs")]
    [Tooltip("the input action asset")]
    [SerializeField] InputActionAsset m_Inputs;

    [Tooltip("the input action for the intro")]
    [SerializeField] InputActionReference m_IntroInput;

    // -- refs --
    [Header("refs")]
    [Tooltip("the player's character")]
    [SerializeField] DisconeCharacterVariable m_CurrentCharacter;

    [Tooltip("if the eyes should be closed")]
    [SerializeField] BoolVariable m_IsClosingEyes;

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
    void Awake() {
        // start with your eyes closed
        m_IsClosingEyes.Value = true;

        // disable all input maps except the intro
        foreach (var map in m_Inputs.actionMaps) {
            map.Disable();
        }

        m_IntroInput.action.actionMap.Enable();
    }

    void Start() {
        m_Subscriptions.Add(m_Store.LoadFinished, OnLoadFinished);
    }

    void Update() {
        m_Delay.Tick();

        if (m_IntroInput.action.WasReleasedThisFrame()) {
            OpenEyes();
        }

        if (m_Delay.IsComplete && !m_CurrentCharacter.Value.Character.State.IsIdle) {
            Finish();
        }
    }

    // -- commands --
    void OpenEyes() {
        // enable all input maps except the intro
        foreach (var map in m_Inputs.actionMaps) {
            map.Enable();
        }

        m_IntroInput.action.actionMap.Disable();

        // open the eyes
        m_Delay.Start();
        m_IsClosingEyes.Value = false;
    }

    /// finish the sequnce and destroy it
    void Finish() {
        // blend to the game camera
        m_IntroCamera.SetActive(false);

        // signal the end of the intro
        m_IntroEnded.Raise();

        // ...
        Destroy(this);
    }

    // -- events --
    void OnLoadFinished() {
        if (!m_Store.Player.HasData) {
            m_IntroCamera.SetActive(true);
        } else {
            OpenEyes();
            Finish();
        }
    }
}

}