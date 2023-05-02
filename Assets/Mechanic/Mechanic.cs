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
        Debug.Log($"subs {m_Subscriptions} {m_IsEyelidClosed} {m_IsEyelidClosed.Changed}");
        m_Subscriptions
            .Add(m_IsEyelidClosed.Changed, OnEyelidClosedChanged);
    }

    void Update() {
        if (m_Delay.IsActive) {
            m_Delay.Tick();

            if (m_Delay.IsComplete) {
                m_DialogueRunner.StartDialogue(m_Node);
            }
        }
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    // -- events --
    /// .
    void OnEyelidClosedChanged(bool isEyelidClosed) {
        if (!isEyelidClosed) {
            m_Delay.Cancel();
        } else {
            m_Delay.Start();
        }
    }
}

}