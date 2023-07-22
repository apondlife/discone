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

    [Tooltip("the current delay")]
    [ReadOnly]
    [SerializeField] float m_Delay;

    // -- tuning --
    [Header("tuning")]
    [Tooltip("the fixed delay accumulated on eye close")]
    [SerializeField] float m_DelayBonus;

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
    /// if delay is currently accumulating
    bool m_IsDelayAccumulating;

    /// a map of nodes by name
    Dictionary<string, MechanicNode> m_Nodes;

    /// a bag of event subscriptions
    DisposeBag m_Subscriptions = new DisposeBag();

    // -- lifecycle --
    void Start() {
        // set props
        m_Nodes = InitNodes();

        // TODO: unity deserializes the default value of a string as "" instead of null?
        m_Node = m_Node == "" ? null : m_Node;
        m_TreeRoot = m_TreeRoot == "" ? null : m_TreeRoot;

        // bind events
        m_Subscriptions
            .Add(m_DialogueRunner.onNodeStart, OnNodeWillStart)
            .Add(m_DialogueRunner.onNodeComplete, OnNodeDidComplete)
            .Add(m_JumpToNode, OnJumpToNode)
            .Add(m_IsEyelidClosed_Changed, OnEyelidClosedChanged)
            .Add(m_Intro_SequenceEnded, OnIntroSequenceEnded)
            .Add(m_SetBirthplaceStep, OnSetBirthplaceStep);
    }

    void Update() {
        // accumulate delay
        if (m_IsDelayAccumulating) {
            m_Delay += Time.deltaTime;
        }
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// set the current node
    void SwitchNode(string nodeName) {
        m_Node = nodeName;
    }

    /// finish the current node, and start the next one if necessary
    void FinishNode(string nodeName) {
        var node = m_Nodes.Get(nodeName);

        // if something else didn't switch the node, set the next node
        if (nodeName == m_Node && node.Next != null) {
            SwitchNode(node.Next);
        }

        // if this isn't the last in a sequence, keep going
        if (!node.IsLast) {
            StartCoroutine(ContinueToNextNode());
        }

        // if this node auto hides on complete, hide now
        if (node.isHiding) {
            HideDialogue();
        }
    }

    /// continue to the next line
    /// NOTE: this needs to be in a coroutine because it's called from
    /// onNodeComplete, which is invoked from StopDialogue, and StartDialogue
    /// then stops dialogue, &c. we need to escape the call stack.
    IEnumerator ContinueToNextNode() {
        yield return null;
        StartDialogue(isContinue: true);
    }

    /// .
    void JumpToNode(string node, bool isContinue = false) {
        Debug.Log(Tag.Mechanic.F($"jump: {node}"));
        SwitchNode(node);
        StartDialogue(isContinue);
    }

    /// .
    void StartDialogue(bool isContinue = false) {
        var nodeName = m_Node;

        // if we're starting in a dialogue tree, begin from the root
        if (!isContinue && IsInTree) {
            nodeName = m_TreeRoot;
        }

        // interrupt any existing dialogue
        StopDialogue();

        // and start the new dialogue
        if (nodeName == null) {
            Debug.LogWarning(Tag.Mechanic.F($"tried to start dialogue w/ no node set"));
            return;
        }

        if (!isContinue) {
            Debug.Log(Tag.Mechanic.F($"strt: {nodeName}"));
        }

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

    /// .
    void SetVariable(string key, string value) {
        Debug.Log(Tag.Mechanic.F($"set var {key} => {value}"));
        m_DialogueRunner.VariableStorage.SetValue(key, value);
    }

    // -- c/tree
    /// set the current tree root
    void SwitchTreeRoot(string nodeName) {
        m_TreeRoot = nodeName;
    }

    /// start a node while in a tree
    void StartTreeNode(string nodeName) {
        var node = m_Nodes.Get(nodeName);

        // if this node is tree structural, it's not content; ignore it
        if (node.IsBranching) {
            return;
        }

        // if this is the first time through the tree, start at the first
        // content node
        if (m_Node == m_TreeRoot) {
            SwitchNode(nodeName);
        }

        // if this is the farthest node we've reached, let's play it
        if (nodeName == m_Node) {
            return;
        }

        // otherwise, lookahead to see if that node is on this branch
        var nextName = nodeName;
        var nextNode = node;

        while (nextName != m_Node && !nextNode.IsLeaf) {
            nextName = nextNode.Next;

            var hasNext = m_Nodes.TryGetValue(nextName, out nextNode);
            if (!hasNext) {
                Debug.LogError(Tag.Mechanic.F($"found branch w/ broken connection and/or last node"));
            }
        }

        // if we found the leaf of a different branch, we diverged. this is the new current node
        if (nextName != m_Node) {
            SwitchNode(nodeName);
        }
        // otherwise, we found a node farther down the branch. start it immediately
        else {
            JumpToNode(nextName, isContinue: true);
        }
    }

    /// finish a node in a dialogue tree
    void FinishTreeNode(string nodeName) {
        var node = m_Nodes.Get(nodeName);

        // if this node is tree structural, it's not content; ignore it
        if (node.IsBranching) {
            return;
        }

        // if this is not the current node, ignore it
        if (nodeName != m_Node) {
            Debug.LogError(Tag.Mechanic.F($"not current node {nodeName} vs {m_Node}"));
            return;
        }

        FinishNode(nodeName);
    }

    // -- c/yarn
    /// set the next node
    [YarnCommand("then")]
    public void Then(string nodeName) {
        Debug.Log(Tag.Mechanic.F($"then: {nodeName}"));
        SwitchNode(nodeName);
    }

    /// wait for an aggregated number of seconds
    [YarnCommand("idle")]
    public IEnumerator Idle(float duration) {
        // start accumulating delay
        m_IsDelayAccumulating = true;

        // wait however much longer we need to
        var remaining = Mathf.Max(duration - m_Delay, 0f);
        yield return new WaitForSeconds(remaining);

        // reset delay if we finish
        m_Delay = 0f;
        m_IsDelayAccumulating = false;
    }

    // -- queries --
    /// if we're in a dialgoue tree
    bool IsInTree {
        get => m_TreeRoot != null;
    }

    // -- events --
    /// when a dialogue node is about to start
    void OnNodeWillStart(string nodeName) {
        var node = m_Nodes.Get(nodeName);

        // clear tree root if we start a node w/ a different scope
        if (IsInTree && !m_TreeRoot.StartsWith(node.Scope)) {
            SwitchTreeRoot(null);
        }

        // if this is a tree, start the tree
        if (!IsInTree && node.IsTree) {
            SwitchTreeRoot(nodeName);
        }

        if (IsInTree) {
            StartTreeNode(nodeName);
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
        SetVariable(MechanicBirthplaceStep.Name, step);
    }

    /// .
    void OnEyelidClosedChanged(bool isEyelidClosed) {
        // show dialogue; add bonus delay, but don't start accumulating until idle command
        if (isEyelidClosed) {
            m_Delay += m_DelayBonus;
            StartDialogue();
        }
        // hide dialogue; stop accumulating when opening eyes
        else {
            m_IsDelayAccumulating = false;
            HideDialogue();
        }
    }

    /// .
    void OnIntroSequenceEnded() {
        JumpToNode(m_StartNode);
    }

    // -- factories --
    /// build a map of node names to node metadata
    Dictionary<string, MechanicNode> InitNodes() {
        var nodes = new Dictionary<string, MechanicNode>();

        // track curr name and prefix to iterate in pairs
        var currName = (string)null;
        var currScope = (string)null;

        // for each node
        foreach (var nextName in m_DialogueRunner.Dialogue.NodeNames) {
            // get the scope
            var nextScope = nextName.SubstringUntil('_');

            // wait for a pair
            if (currName == null) {
                currName = nextName;
                currScope = nextScope;
                continue;
            }

            // parse tags
            var tags = (MechanicNode.Tag)0;
            foreach (var tag in m_DialogueRunner.GetTagsForNode(currName)) {
                tags |= tag switch {
                    "#last" => MechanicNode.Tag.Last,
                    "#hide" => MechanicNode.Tag.Hide,
                    "#tree" => MechanicNode.Tag.Tree,
                    "#fork" => MechanicNode.Tag.Fork,
                    "#leaf" => MechanicNode.Tag.Leaf,
                    _       => 0
                };
            }

            // add node
            var hasNext = (
                currScope == nextScope &&
                (tags & MechanicNode.TreeLike) == 0
            );

            nodes.Add(currName, new MechanicNode(
                next: hasNext ? nextName : null,
                tags,
                currScope
            ));

            // bookkeeping
            currName = nextName;
            currScope = nextScope;
        }

        return nodes;
    }
}

}