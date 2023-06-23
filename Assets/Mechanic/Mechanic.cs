using UnityAtoms.BaseAtoms;
using UnityEngine;
using Yarn.Unity;

namespace Discone {

/// the mechanic that speaks to the player
sealed class Mechanic: MonoBehaviour {
    // -- state --
    [Header("state")]
    [Tooltip("the node name")]
    [ReadOnly]
    [SerializeField] string m_Node;

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the node to start w/ after the intro")]
    [SerializeField] string m_StartNode;

    // -- refs --
    [Header("refs")]
    [Tooltip(".")]
    [SerializeField] DialogueRunner m_DialogueRunner;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("an event that jumps to a new dialogue node immediately")]
    [SerializeField] StringEvent m_JumpToNode;

    [Tooltip("an event that sets the current birthplace step")]
    [SerializeField] StringEvent m_SetBirthplaceStep;

    [Tooltip("an event when the eyelid just closes or starts to open")]
    [SerializeField] BoolEvent m_IsEyelidClosed_Changed;

    [Tooltip("an event when the intro sequence finishes")]
    [SerializeField] VoidEvent m_Intro_SequenceEnded;

    // -- props --
    /// a bag of event subscriptions
    DisposeBag m_Subscriptions = new DisposeBag();

    // -- lifecycle --
    void Start() {
        m_DialogueRunner.VariableStorage.SetValue(
            MechanicBirthplaceStep.Name,
            MechanicBirthplaceStep.InitialValue
        );

        m_Subscriptions
            .Add(m_JumpToNode, OnJumpToNode)
            .Add(m_IsEyelidClosed_Changed, OnEyelidClosedChanged)
            .Add(m_Intro_SequenceEnded, OnIntroSequenceEnded)
            .Add(m_SetBirthplaceStep, OnSetBirthplaceStep);
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
    void JumpToNode(string node) {
        Debug.Log($"[mechnk] jump: {node}");
        SwitchNode(node);
        StartDialogue();
    }

    /// .
    void StartDialogue() {
        // interrupt any existing dialogue
        StopDialogue();

        // and start the new dialogue
        if (string.IsNullOrEmpty(m_Node)) {
            Debug.LogWarning($"[mechnk] tried to start dialogue w/ no node set");
            return;
        }

        m_DialogueRunner.StartDialogue(m_Node);
    }

    /// .
    void StopDialogue() {
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
        Debug.Log($"[mechnk] then: {nodeName}");
        SwitchNode(nodeName);
    }

    // -- events --
    /// when the mechanic should jump to a named node
    void OnJumpToNode(string nodeName) {
        JumpToNode(nodeName);
    }

    /// .
    void OnSetBirthplaceStep(string step) {
        Debug.Log($"[mechnk] birthplace: {step}");
        m_DialogueRunner.VariableStorage.SetValue(
            MechanicBirthplaceStep.Name,
            step
        );
    }

    /// .
    void OnEyelidClosedChanged(bool isEyelidClosed) {
        if (isEyelidClosed) {
            StartDialogue();
        } else {
            StopDialogue();
        }
    }

    /// .
    void OnIntroSequenceEnded() {
        JumpToNode(m_StartNode);
    }
}

}