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

    [Tooltip("the delay before showing the first line of dialogue (hack)")]
    [SerializeField] EaseTimer m_DialogueDelay;

    [Tooltip("the delay before finishing the intro")]
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

    [Tooltip("the character's rotation reference for the inital shot")]
    [SerializeField] Transform m_CharacterRotationReference;

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

        // bind events
        m_Subscriptions
            .Add(m_Store.LoadFinished, OnLoadFinished)
            // HACK HACK HACK: do this better later
            // this is so that the follow camera points towards
            // a different direction then ice creams orientation
            .Add<DisconeCharacterPair>(m_CurrentCharacter.ChangedWithHistory, _ => {
                this.DoAfterTime(0.1f, () => {
                    var initialState = m_CurrentCharacter.Value.Character.State.Curr.Copy();
                    initialState.Forward = m_CharacterRotationReference.forward;
                    m_CurrentCharacter.Value.Character.ForceState(initialState);
                });
            });
    }

    void Update() {
        // show start dialogue
        if (m_DialogueDelay.IsActive) {
            m_DialogueDelay.Tick();

            if (m_DialogueDelay.IsComplete) {
                m_Mechanic_JumpToNode.Raise(m_Mechanic_StartNode);
            }
        }

        // delay intro to ignore the input being pressed when the game starts
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
    /// begin the intro sequence
    void Init() {
        // start intro dialogue
        m_DialogueDelay.Start();

        // switch to the intro camera
        m_IntroCamera.SetActive(true);
    }

    /// open the player's eyes
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
            Init();
        } else {
            OpenEyes();
            Finish();
        }
    }
}

}