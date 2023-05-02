using UnityAtoms.BaseAtoms;
using UnityEngine;
using Yarn.Unity;

namespace Discone {

/// the mechanic's eyelid dialogue
class Mechanic: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the node name")]
    [SerializeField] string m_Node;

    [Tooltip("the delay before running a line")]
    [SerializeField] ThirdPerson.EaseTimer m_Delay;

    // -- refs --
    [Header("refs")]
    [Tooltip(".")]
    [SerializeField] DialogueRunner m_DialogueRunner;

    [Tooltip("an event when the eyelid just closes or starts to open")]
    [SerializeField] BoolVariable m_IsEyelidClosed;

    /// a bag of event subscriptions
    DisposeBag m_Subscriptions = new DisposeBag();

    // -- lifecycle --
    void Start() {
        m_Subscriptions
            .Add(m_IsEyelidClosed.Changed, OnEyelidClosedChanged);
    }

    void FixedUpdate() {
        if (m_Delay.IsActive) {
            m_Delay.Tick();

            if (m_Delay.IsComplete) {
                StartDialogue();
            }
        }
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// .
    void StartDelay() {
        m_Delay.Start();
    }

    /// .
    void StartDialogue() {
        m_DialogueRunner.StartDialogue(m_Node);
    }

    /// .
    void StopDialogue() {
        m_Delay.Cancel();

        // clean up the dialogue runner
        if (m_DialogueRunner.IsDialogueRunning) {
            m_DialogueRunner.Stop();

            // manually call dialogue complete on the views, since yarn explicitly
            // does not when you call stop?
            foreach (var dialogueView in m_DialogueRunner.dialogueViews) {
                dialogueView.DialogueComplete();
            }
        }
    }

    // -- events --
    /// .
    void OnEyelidClosedChanged(bool isEyelidClosed) {
        if (isEyelidClosed) {
            StartDelay();
        } else {
            StopDialogue();
        }
    }
}

}