using ThirdPerson;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Yarn.Unity;
using CharacterController = ThirdPerson.CharacterController;

namespace Discone {

public class IntroSequence: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the delay before starting the intro")]
    [SerializeField] EaseTimer m_StartDelay;

    [Tooltip("the delay before showing the first line of dialogue (hack)")]
    [SerializeField] EaseTimer m_DialogueDelay;

    [Tooltip("the delay before finishing the intro")]
    [SerializeField] EaseTimer m_FinishDelay;

    // -- mechanic --
    [Header("mechanic")]
    [Tooltip("the mechanic yarn project")]
    [SerializeField] YarnProject m_Mechanic;

    [Tooltip("the mechanic node to play on start")]
    [YarnNode(nameof(m_Mechanic))]
    [SerializeField] string m_Mechanic_StartNode;

    [Tooltip("the mechanic node to play on input")]
    [YarnNode(nameof(m_Mechanic))]
    [SerializeField] string m_Mechanic_InputNode;

    [Tooltip("the mechanic node to play on eyes open")]
    [YarnNode(nameof(m_Mechanic))]
    [SerializeField] string m_Mechanic_EndNode;

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

    [Tooltip("the intro retrigger camera")]
    [SerializeField] GameObject m_IntroCameraRetrigger;

    [FormerlySerializedAs("m_InitialRotation")]
    [Tooltip("the character's rotation reference for the inital shot")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_CharacterRotationReference")]
    [SerializeField] Transform m_InitialTransform;

    [Tooltip("the shared data store")]
    [SerializeField] Store m_Store;

    // -- dispatched --
    [Header("subscribed")]
    [Tooltip("when the dream is over")]
    [SerializeField] VoidEvent m_DreamEnded;

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
    readonly DisposeBag m_Subscriptions = new();

    // -- lifecycle --
    void Start() {
        // bind events
        m_Subscriptions
            .Add(m_Store.LoadFinished, OnLoadFinished)
            .Add(m_DreamEnded, OnDreamEnded);
    }

    // AAA
    void OnDreamEnded() {
        // start intro dialogue
        m_DialogueDelay.Start();

        // switch to the intro camera
        m_IntroCamera.SetActive(true);

        // start with your eyes closed
        m_IsClosingEyes.Value = true;

        // disable all input maps except the intro
        foreach (var map in m_Inputs.actionMaps) {
            map.Disable();
        }

        m_IntroInput.action.actionMap.Enable();

        // AAA: old start
        m_StartDelay.Start();

        // HACK: do this better later this is so that the follow camera points
        // towards a different direction then ice creams orientation
        // set initial character state
        var initialState = m_CurrentCharacter.Value.Character.State.Curr.Copy();
        initialState.Position = m_InitialTransform.position;
        initialState.Forward = m_InitialTransform.forward;
        m_CurrentCharacter.Value.Character.ForceState(initialState);

       // create checkpoint

    }

    void Update() {
        // show start dialogue
        if (m_DialogueDelay.TryComplete()) {
            m_Mechanic_JumpToNode.Raise(m_Mechanic_StartNode);
        }

        // delay intro to ignore the input being pressed when the game starts
        if (m_StartDelay.IsActive && m_StartDelay.TryComplete()) {
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

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// open the player's eyes
    void OpenEyes() {
        // enable all input maps except the intro
        foreach (var map in m_Inputs.actionMaps) {
            map.Enable();
        }

        m_IntroInput.action.actionMap.Disable();

        // jump to the end node
        m_Mechanic_JumpToNode.Raise(m_Mechanic_EndNode);

        // open the eyes
        m_FinishDelay.Start();
        m_IsClosingEyes.Value = false;
    }

    /// finish the sequence and destroy it
    void Finish() {
        // blend to the game camera
        m_IntroCamera.SetActive(false);

        // signal the end of the intro
        m_IntroEnded.Raise();

        // enable retriggering this camera
        m_IntroCameraRetrigger.SetActive(true);

        // ...
        Destroy(this);
    }

    // -- events --
    void OnLoadFinished() {
        if (m_Store.Player.HasData) {
            OpenEyes();
            Finish();
        }
    }
}

}