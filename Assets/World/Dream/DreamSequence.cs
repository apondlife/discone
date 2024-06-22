using System;
using Soil;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Serialization;
using Yarn.Unity;

namespace Discone {

// TODO: optional step when falling from platform
sealed class DreamSequence: MonoBehaviour {
    [Serializable]
    record Step {
        [Tooltip("the step's trigger")]
        public DreamSequenceTrigger Trigger;

        [Tooltip("an optional timeout to fire the step")]
        public EaseTimer Timeout;

        [Tooltip("the mechanic node to play on start")]
        [YarnNode(nameof(m_Mechanic))]
        public string Mechanic_StartNode;
    }

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the initial flower position")]
    [SerializeField] Transform m_StartFlowerPos;

    [FormerlySerializedAs("m_Camera")]
    [Tooltip("the camera for the opening shot")]
    [SerializeField] GameObject m_StartCamera;

    [Tooltip("the sequence of steps")]
    [SerializeField] Step[] m_Steps;

    // -- dialogue --
    [Header("dialogue")]
    [Tooltip("the mechanic yarn project")]
    [SerializeField] YarnProject m_Mechanic;

    [FormerlySerializedAs("m_StartNode")]
    [Tooltip("the initial dialogue node")]
    [YarnNode(nameof(m_Mechanic))]
    [SerializeField] string m_Mechanic_StartNode;

    [Tooltip("switch the current mechanic node")]
    [SerializeField] StringEvent m_Mechanic_Switch;

    [FormerlySerializedAs("m_Mechanic_JumpToNode")]
    [Tooltip("jump to a new mechanic node")]
    [SerializeField] StringEvent m_Mechanic_Jump;

    // -- dispatched --
    [Header("dispatched")]
    [Tooltip("when the dream ends")]
    [SerializeField] VoidEvent m_DreamEnded;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("when a game step starts")]
    [SerializeField] GameStepEvent m_GameStep_Started;

    // -- refs --
    [Header("refs")]
    [Tooltip("a reference to the current character")]
    [SerializeField] CharacterVariable m_CurrentCharacter;

    [Tooltip("if the player's eyes are closed")]
    [SerializeField] BoolReference m_IsEyelidClosed;

    /// -- props --
    /// the current step in the sequence
    int m_StepIndex;

    /// a set of event subscriptions
    readonly DisposeBag m_Subscriptions = new();

    /// -- lifecycle --
    void Start() {
        // add subscriptions
        m_Subscriptions.Add(m_GameStep_Started, OnGameStepStarted);
    }

    void Update() {
        var character = m_CurrentCharacter.Value;
        if (IsStartCamera && character && character.Input.Curr.AnyMove) {
            OnCharacterMove();
        }

        if (m_StepIndex < m_Steps.Length) {
            var step = m_Steps[m_StepIndex];

            step.Timeout.TryTick();
            if (step.Timeout.IsComplete && !m_IsEyelidClosed) {
                FinishStep();
            }
        }
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();

        // re-enable flowers
        m_CurrentCharacter.Value.Checkpoint.IsBlocked = false;

        // clean up any triggers
        foreach (var step in m_Steps) {
            if (step.Trigger) {
                step.Trigger.Finish();
            }
        }

        // clean up the camera
        m_StartCamera.SetActive(false);
    }

    // -- commands --
    void Init() {
        // plant initial flower
        var character = m_CurrentCharacter.Value;
        character.PlantFlower(Checkpoint.FromTransform(m_StartFlowerPos));

        // block subsequent flowers
        var checkpoint = character.Checkpoint;
        character.Checkpoint.IsBlocked = true;

        // switch to the start dialogue node
        m_Mechanic_Switch.Raise(m_Mechanic_StartNode);

        // init steps
        var i = 0;
        foreach (var step in m_Steps) {
            step.Trigger.OnFire(OnStepTriggerFired);
            step.Trigger.Toggle(i == 0);
            i += 1;
        }

        // bind events
        m_Subscriptions.Add(checkpoint.OnCreate, OnCreateCheckpoint);
    }

    /// starts the current step
    void StartStep() {
        var curr = m_Steps[m_StepIndex];

        // enable the trigger
        curr.Trigger.Toggle(true);

        // start the timeout, if any
        if (!curr.Timeout.IsZero) {
            curr.Timeout.Start();
        }
    }

    /// finish the current step and start the next one, if any
    void FinishStep() {
        // complete this step
        var curr = FindCurrStep();
        curr.Trigger.Finish();

        // show the dialogue
        m_Mechanic_Jump.Raise(curr.Mechanic_StartNode);

        // advance to the next step
        m_StepIndex += 1;

        // and start it, if any
        var next = FindCurrStep();
        if (next != null) {
            StartStep();
        }
    }

    /// finish the sequence
    void Finish() {
        Destroy(this);
    }

    // -- queries --
    /// get the current step, if any
    Step FindCurrStep() {
        return m_StepIndex < m_Steps.Length ? m_Steps[m_StepIndex] : null;
    }

    /// if we are looking at the game from the initial camera
    bool IsStartCamera {
        get => m_StartCamera.activeSelf;
    }

    // -- events --
    /// when the character initially moves
    void OnCharacterMove() {
        m_StartCamera.SetActive(false);
        StartStep();
    }

    /// when the step trigger fires
    void OnStepTriggerFired() {
        FinishStep();
    }

    /// when the final checkpoint is created
    void OnCreateCheckpoint(Checkpoint _) {
        m_DreamEnded.Raise();
        Finish();
    }

    /// when a new game step starts
    void OnGameStepStarted(GameStep step) {
        if (step == GameStep.Dream) {
            Init();
        } else if (step > GameStep.Dream) {
            Finish();
        }
    }
}

}