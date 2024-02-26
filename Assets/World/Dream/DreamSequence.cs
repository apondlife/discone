using System;
using System.Security.Cryptography;
using ThirdPerson;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Serialization;
using Yarn.Unity;

namespace Discone {

// TODO: timer for move step
// TODO: optional step when falling from platform
sealed class DreamSequence: MonoBehaviour {
    [Serializable]
    record Step {
        [Tooltip("the step's trigger")]
        public DreamSequenceTrigger Trigger;

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

    void OnDestroy() {
        m_CurrentCharacter.Value.Checkpoint.IsBlocked = false;

        foreach (var step in m_Steps) {
            if (step.Trigger) {
                Destroy(step.Trigger.gameObject);
            }
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
            step.Trigger.OnFire(OnStep);
            step.Trigger.gameObject.SetActive(i == 0);
            i += 1;
        }

        // bind events
        m_Subscriptions
            .Add(checkpoint.OnCreate, OnCreateCheckpoint);
    }

    // -- events --
    void OnCurrentCharacterChanged(DisconeCharacterPair _) {
        Debug.Log("[dream] OnCharacterChanged");
        if (m_Store.Player.HasData) {
            Destroy(this);
            return;
        }

        Init();
    }

    void OnStep() {
        var curr = m_Steps[m_StepIndex];
        Destroy(curr.Trigger.gameObject);

        m_Mechanic_JumpToNode.Raise(curr.Mechanic_StartNode);
        m_StepIndex += 1;

        if (m_StepIndex < m_Steps.Length) {
            var next = m_Steps[m_StepIndex];
            next.Trigger.gameObject.SetActive(true);
        }
    }

    void OnCreateCheckpoint(Checkpoint _) {
        m_CurrentCharacter.Value.Checkpoint.IsBlocked = false;
        m_DreamEnded.Raise();
        Destroy(this);
    }
}

}