using System;
using ThirdPerson;
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
    [Tooltip("the sequence of steps")]
    [SerializeField] Step[] m_Steps;

    // -- refs --
    [Header("refs")]
    [FormerlySerializedAs("m_FlowerPosition")]
    [Tooltip("the initial flower position")]
    [SerializeField] Transform m_InitialFlowerPos;

    [Tooltip("a reference to the current character")]
    [SerializeField] DisconeCharacterVariable m_CurrentCharacter;

    [Tooltip("the shared data store")]
    [SerializeField] Store m_Store;

    [Tooltip("the mechanic yarn project")]
    [SerializeField] YarnProject m_Mechanic;

    [Tooltip("the initial camera")]
    [SerializeField] GameObject m_Camera;

    // -- dispatched --
    [Header("dispatched")]
    [Tooltip("when the mechanic should jump to a node")]
    [SerializeField] StringEvent m_Mechanic_JumpToNode;

    [Tooltip("when the dream ends")]
    [SerializeField] VoidEvent m_DreamEnded;

    /// -- props --
    /// the current step in the sequence
    int m_StepIndex;

    /// the set of event subscriptions
    readonly DisposeBag m_Subscriptions = new();

    /// -- lifecycle --
    void Start() {
        m_Subscriptions
            .Add(m_CurrentCharacter.ChangedWithHistory, OnCurrentCharacterChanged);
    }

    void Update() {
        if (m_StepIndex < m_Steps.Length) {
            var step = m_Steps[m_StepIndex];
            if (step.Timeout.TryComplete()) {
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

        // on first session, finish the dream
        if (!m_Store.Player.HasData) {
            m_DreamEnded.Raise();
        }
    }

    // -- commands --
    void Init() {
        var character = m_CurrentCharacter.Value;
        character.PlantFlower(Checkpoint.FromTransform(m_InitialFlowerPos));

        var checkpoint = character.Checkpoint;
        character.Checkpoint.IsBlocked = true;

        // init steps
        var i = 0;
        foreach (var step in m_Steps) {
            step.Trigger.OnFire(OnStepTriggerFired);
            step.Trigger.Toggle(i == 0);
            i += 1;
        }

        // bind events
        m_Subscriptions
            .Add(checkpoint.OnCreate, OnCreateCheckpoint);

        character.Character.Events
            .Once(CharacterEvent.Move, OnCharacterMove);
    }

    /// starts the current step
    public void StartStep() {
        var curr = m_Steps[m_StepIndex];

        // enable the trigger
        curr.Trigger.Toggle(true);

        // start the timeout, if any
        if (!curr.Timeout.IsZero) {
            curr.Timeout.Start();
        }
    }

    /// finish the current step and start the next one, if any
    public void FinishStep() {
        // complete this step
        var curr = FindCurrStep();
        curr.Trigger.Finish();

        // show the dialogue
        m_Mechanic_JumpToNode.Raise(curr.Mechanic_StartNode);

        // advance to the next step
        m_StepIndex += 1;

        // and start it, if any
        var next = FindCurrStep();
        if (next != null) {
            StartStep();
        }
    }

    // -- queries --
    /// get the current step, if any
    Step FindCurrStep() {
        return m_StepIndex < m_Steps.Length ? m_Steps[m_StepIndex] : null;
    }

    // -- events --
    /// when the initial character loads
    void OnCurrentCharacterChanged(DisconeCharacterPair _) {
        if (m_Store.Player.HasData) {
            Destroy(this);
            return;
        }

        Init();
    }

    /// when the character initially moves
    void OnCharacterMove() {
        m_Camera.SetActive(false);
        StartStep();
    }

    /// when the step trigger fires
    void OnStepTriggerFired() {
        FinishStep();
    }

    /// when the final checkpoint is created
    void OnCreateCheckpoint(Checkpoint _) {
        m_CurrentCharacter.Value.Checkpoint.IsBlocked = false;
        Destroy(this);
    }
}

}