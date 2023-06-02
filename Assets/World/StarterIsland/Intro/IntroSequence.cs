using ThirdPerson;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Discone {

public class IntroSequence: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the delay before starting the intro")]
    [SerializeField] EaseTimer m_StartDelay;

    [Tooltip("the delay before finishing the intro")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_Delay")]
    [SerializeField] EaseTimer m_FinishDelay;

    [Tooltip("the mechanic node to play on start")]
    [SerializeField] StringReference m_Mechanic_StartNode;

    [Tooltip("the mechanic node to play on input")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_Mechanic_Node")]
    [SerializeField] StringReference m_Mechanic_InputNode;

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

    [Tooltip("the intro camera")]
    [SerializeField] GameObject m_IntroCamera;

    [Tooltip("the shared data store")]
    [SerializeField] Store m_Store;

    // -- dispatched --
    [Header("dispatched")]
    [Tooltip("when the intro is over")]
    [SerializeField] VoidEvent m_IntroEnded;

    [Tooltip("when the mechanic should jump to a node")]
    [SerializeField] StringEvent m_Mechanic_JumpToNode;

    // -- props --
    /// if the input was performed
    bool m_WasPerformed;

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
        m_StartDelay.Start();

        // start intro dialogue
        m_Mechanic_JumpToNode.Raise(m_Mechanic_StartNode);

        // bind events
        m_Subscriptions.Add(m_Store.LoadFinished, OnLoadFinished);
    }

    void Update() {
        // wait to start to ignore the input being pressed when the game starts
        m_StartDelay.Tick();
        if (!m_StartDelay.IsComplete) {
            return;
        }

        var input = m_IntroInput.action;

        // play input dialogue
        if (input.WasPressedThisFrame()) {
            m_Mechanic_JumpToNode.Raise(m_Mechanic_InputNode);
        }

        // when the hold is performed
        if (!m_WasPerformed) {
            m_WasPerformed = input.phase == InputActionPhase.Performed;
        }

        if (input.WasReleasedThisFrame() && m_WasPerformed) {
            OpenEyes();
        }

        // finish the intro once the character moves
        m_FinishDelay.Tick();
        if (m_FinishDelay.IsComplete && !m_CurrentCharacter.Value.Character.State.IsIdle) {
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
        m_FinishDelay.Start();
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