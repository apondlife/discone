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

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("an event that jumps to a new dialogue node immediately")]
    [SerializeField] StringEvent m_JumpToNode;

    [Tooltip("an event when the eyelid just closes or starts to open")]
    [SerializeField] BoolEvent m_IsEyelidClosed_Changed;

    // -- props --
    /// a bag of event subscriptions
    DisposeBag m_Subscriptions = new DisposeBag();

    // -- lifecycle --
    void Start() {
        m_Subscriptions
            .Add(m_JumpToNode, OnJumpToNode)
            .Add(m_IsEyelidClosed_Changed, OnEyelidClosedChanged);
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
    void SwitchNode(string node) {
        // TODO: save this to disk
        m_Node = node;
    }

    /// .
    void StartDelay() {
        m_Delay.Start();
    }

    /// .
    void StartDialogue() {
        if (string.IsNullOrEmpty(m_Node)) {
            Debug.LogWarning($"[mechnik] tried to start dialogue w/ no node set");
            return;
        }

        m_DialogueRunner.StartDialogue(m_Node);
    }

    /// .
    void StopDialogue() {
        m_Delay.Cancel();

        // clean up the dialogue runner
        if (m_DialogueRunner.IsDialogueRunning) {
            m_DialogueRunner.StopAllCoroutines();
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
        Debug.Log($"[mechnik] then: {nodeName}");
        SwitchNode(nodeName);
    }

    // -- events --
    /// when the mechanic should jump to a named node
    void OnJumpToNode(string nodeName) {
        Debug.Log($"[mechnik] jump: {nodeName}");
        StopDialogue();
        SwitchNode(nodeName);
        StartDialogue();
    }

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