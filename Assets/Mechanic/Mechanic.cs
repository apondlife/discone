using UnityAtoms.BaseAtoms;
using UnityEngine;
using Yarn.Unity;

namespace Discone {

/// the mechanic that speaks to the player
sealed class Mechanic: MonoBehaviour {
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

            // manually call DialogueComplete on views, since yarn explicitly
            // does not when stop is called.
            foreach (var dialogueView in m_DialogueRunner.dialogueViews) {
                dialogueView.DialogueComplete();
            }
        }
    }

    // -- c/yarn
    [YarnCommand("then")]
    public void Then(string nodeName) {
        // TODO: save this to disk
        m_Node = nodeName;
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