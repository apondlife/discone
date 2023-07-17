using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using Yarn.Unity;

namespace Discone {

/// the mechanic that speaks to the player
sealed class Mechanic: MonoBehaviour {
    // -- state --
    // TODO: save this to disk
    [Header("state")]
    [Tooltip("the node name")]
    [ReadOnly]
    [SerializeField] string m_Node;

    [Tooltip("the name of the root node of the current tree, if any")]
    [ReadOnly]
    [SerializeField] string m_TreeRoot;

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
    /// a map of nodes by name
    Dictionary<string, MechanicNode> m_Nodes;

    /// a bag of event subscriptions
    DisposeBag m_Subscriptions = new DisposeBag();

    // -- lifecycle --
    void Start() {
        // set props
        m_Nodes = InitNodes();

        // bind events
        m_Subscriptions
            .Add(m_DialogueRunner.onNodeStart, OnNodeWillStart)
            .Add(m_DialogueRunner.onNodeComplete, OnNodeDidComplete)
            .Add(m_JumpToNode, OnJumpToNode)
            .Add(m_IsEyelidClosed_Changed, OnEyelidClosedChanged)
            .Add(m_Intro_SequenceEnded, OnIntroSequenceEnded)
            .Add(m_SetBirthplaceStep, OnSetBirthplaceStep);
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// set the current node
    void SwitchNode(string nodeName) {
        m_Node = nodeName;

        // clear tree state if we're no longer in a tree
        if (m_TreeRoot != null && !IsInTree) {
            m_TreeRoot = null;
        }
    }

    /// finish the current node, and start the next one if necessary
    void FinishNode(string nodeName) {
        var node = m_Nodes.Get(nodeName);

        // if this is the root of a tree, stop here
        if (node.IsTree) {
            m_TreeRoot = nodeName;
            return;
        }

        // if something else didn't switch the node, set the next node
        if (nodeName != m_Node && node.Next != null) {
            SwitchNode(node.Next);
        }

        // if this isn't the last in a sequence, keep going
        if (!node.IsLast) {
            StartCoroutine(ContinueToNextNode());
        }
    }

    /// continue to the next line
    /// NOTE: this needs to be in a coroutine because it's called from
    /// onNodeComplete, which is invoked from StopDialogue, and StartDialogue
    /// then stops dialogue, &c. we need to escape that call stack.
    IEnumerator ContinueToNextNode() {
        yield return null;
        StartDialogue(isContinue: true);
    }

    /// .
    void JumpToNode(string node) {
        Debug.Log(Tag.Mechanic.F($"jump: {node}"));
        SwitchNode(node);
        StartDialogue();
    }

    /// .
    void StartDialogue(bool isContinue = false) {
        var nodeName = m_Node;

        // if we're continuing through a tree, start whatever is in the tree
        if (isContinue && IsInTree) {
            nodeName = m_TreeRoot;
        }

        // interrupt any existing dialogue
        StopDialogue();

        // and start the new dialogue
        if (string.IsNullOrEmpty(nodeName)) {
            Debug.LogWarning(Tag.Mechanic.F($"tried to start dialogue w/ no node set"));
            return;
        }

        Debug.Log(Tag.Mechanic.F($"start dialogue {nodeName}"));
        m_DialogueRunner.StartDialogue(nodeName);
    }

    /// stop any active dialogue
    void StopDialogue() {
        // clean up the dialogue runner
        if (m_DialogueRunner.IsDialogueRunning) {
            m_DialogueRunner.StopAllCoroutines();
            m_DialogueRunner.Stop();
        }
    }

    /// stop & hide any active dialogue views
    void HideDialogue() {
        StopDialogue();

        foreach (var dialogueView in m_DialogueRunner.dialogueViews) {
            if (dialogueView is Ui.MechanicDialogueView m) {
                m.Hide();
            }
        }
    }

    // -- c/tree
    /// set the current tree node
    void SwitchTreeNode(string nodeName) {
        m_Node = nodeName;
    }

    /// start a node while in a tree
    void TryStartTreeNode(string nodeName) {
        var node = m_Nodes.Get(nodeName);

        // if this node is tree structural, it's not content. ignore it
        if (node.IsTreeLike) {
            return;
        }

        Debug.Log(Tag.Mechanic.F($"will start tree node {nodeName}"));
        // on the first time through the tree, mark this as the current node
        if (m_Node == m_TreeRoot) {
            SwitchNode(nodeName);
        }
    }

    /// finish a node in a dialogue tree
    void FinishTreeNode(string nodeName) {
        var node = m_Nodes.Get(nodeName);

        // if this node is tree structural, it's not content. ignore it
        if (node.IsTreeLike) {
            return;
        }

        // if something else didn't switch the node, set the next tree node
        if (nodeName == m_Node && node.Next != null) {
            SwitchTreeNode(node.Next);
        }

        // if this isn't the last in a sequence, keep going
        if (!node.IsLast) {
            StartCoroutine(ContinueToNextNode());
        }
    }

    // -- c/yarn
    [YarnCommand("then")]
    public void Then(string nodeName) {
        Debug.Log(Tag.Mechanic.F($"then: {nodeName}"));
        SwitchNode(nodeName);
    }

    // -- queries --
    /// if we're in a dialgoue tree
    bool IsInTree {
        get => m_Nodes[m_Node].IsTree;
    }

    /// build a map of node names to node metadata
    Dictionary<string, MechanicNode> InitNodes() {
        var nodes = new Dictionary<string, MechanicNode>();

        // track curr name and prefix to iterate in pairs
        var currName = (string)null;
        var currPrefix = (string)null;

        // for each node
        foreach (var nextName in m_DialogueRunner.Dialogue.NodeNames) {
            // get the prefix namespace
            var nextPrefix = nextName.SubstringUntil('_');

            // wait for a pair
            if (currName == null) {
                currName = nextName;
                currPrefix = nextPrefix;
                continue;
            }

            // parse tags
            var tags = (MechanicNode.Tag)0;
            foreach (var tag in m_DialogueRunner.GetTagsForNode(currName)) {
                tags |= tag switch {
                    "#last" => MechanicNode.Tag.Last,
                    "#tree" => MechanicNode.Tag.Tree,
                    "#stem" => MechanicNode.Tag.Stem,
                    "#leaf" => MechanicNode.Tag.Leaf,
                    _       => 0
                };
            }

            // add node
            var hasNext = (
                currPrefix == nextPrefix &&
                (tags & MechanicNode.TreeLike) == 0
            );

            nodes.Add(currName, new MechanicNode(
                next: hasNext ? nextName : null,
                tags
            ));

            // bookkeeping
            currName = nextName;
            currPrefix = nextPrefix;
        }

        return nodes;
    }

    // -- events --
    /// when a dialogue node is about to start
    void OnNodeWillStart(string nodeName) {
        if (IsInTree) {
            TryStartTreeNode(nodeName);
        }
    }

    /// when a dialogue node finishes
    void OnNodeDidComplete(string nodeName) {
        if (IsInTree) {
            FinishTreeNode(nodeName);
        } else {
            FinishNode(nodeName);
        }
    }

    /// when the mechanic should jump to a named node
    void OnJumpToNode(string nodeName) {
        JumpToNode(nodeName);
    }

    /// .
    void OnSetBirthplaceStep(string step) {
        Debug.Log(Tag.Mechanic.F($"birthplace: {step}"));
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
            HideDialogue();
        }
    }

    /// .
    void OnIntroSequenceEnded() {
        JumpToNode(m_StartNode);
    }
}

}