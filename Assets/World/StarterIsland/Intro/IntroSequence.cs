using Soil;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Yarn.Unity;

namespace Discone {

sealed class IntroSequence: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the delay before finishing the intro")]
    [SerializeField] EaseTimer m_FinishDelay;

    // -- mechanic --
    [Header("mechanic")]
    [Tooltip("the mechanic yarn project")]
    [SerializeField] YarnProject m_Mechanic;

    [Tooltip("the mechanic node to play on eyes open")]
    [YarnNode(nameof(m_Mechanic))]
    [SerializeField] string m_Mechanic_EndNode;

    // -- refs --
    [Header("refs")]
    [Tooltip("the player's character")]
    [SerializeField] DisconeCharacterVariable m_CurrentCharacter;

    [Tooltip("if the eyes should be closed")]
    [SerializeField] BoolVariable m_IsClosingEyes;

    [Tooltip("the intro camera")]
    [SerializeField] GameObject m_IntroCamera;

    [Tooltip("the intro retrigger camera")]
    [SerializeField] GameObject m_IntroCameraRetrigger;

    [Tooltip("the character's rotation reference for the initial shot")]
    [SerializeField] Transform m_InitialTransform;

    [Tooltip("the shared data store")]
    [SerializeField] Store m_Store;

    // -- dispatched --
    [Header("subscribed")]
    [Tooltip("when the dream is over")]
    [SerializeField] VoidEvent m_DreamEnded;

    // -- dispatched --
    [Header("dispatched")]
    [Tooltip("when the intro is over")]
    [SerializeField] VoidEvent m_IntroEnded;

    [Tooltip("when the mechanic should jump to a node")]
    [SerializeField] StringEvent m_Mechanic_JumpToNode;

    // -- props --
    /// if the input was performed
    bool m_WasPerformed;

    /// the set of event subscriptions
    readonly DisposeBag m_Subscriptions = new();

    // -- lifecycle --
    void Start() {
        // bind events
        m_Subscriptions
            .Add(m_Store.LoadFinished, OnLoadFinished)
            .Add(m_DreamEnded, OnDreamEnded);
    }

    void Update() {
        // finish the intro once the character moves
        m_FinishDelay.Tick();

        // we want the character to be able to move after the timer is complete
        if (m_FinishDelay.IsComplete && !m_CurrentCharacter.Value.State.IsIdle) {
            Finish();
        }
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// open the player's eyes
    void OnIsClosingEyesChanged(bool isClosingEyes) {
        if (isClosingEyes) {
            return;
        }

        // jump to the end node
        m_Mechanic_JumpToNode.Raise(m_Mechanic_EndNode);

        // open the eyes
        m_FinishDelay.Start();
    }

    /// finish the sequence and destroy it
    void Finish() {
        // blend to the game camera
        m_IntroCamera.SetActive(false);

        // signal the end of the intro
        m_IntroEnded.Raise();

        // enable retriggering this camera
        m_IntroCameraRetrigger.SetActive(true);

        // ...
        Destroy(this);
    }

    // -- events --
    void OnLoadFinished() {
        if (m_Store.Player.HasData) {
            Finish();
        }
    }

    void OnDreamEnded() {
        // add open eyes subscription
        m_Subscriptions
            .Add(m_IsClosingEyes.Changed, OnIsClosingEyesChanged);

        // switch to the intro camera
        m_IntroCamera.SetActive(true);

        // HACK: do this better later this is so that the follow camera points
        // towards a different direction then ice creams orientation
        var character = m_CurrentCharacter.Value;

        // set initial character state
        var nextState = character.State.Curr.Copy();
        nextState.Position = m_InitialTransform.position;
        nextState.Forward = m_InitialTransform.forward;
        character.ForceState(nextState);

        // TODO: plant flower somewhere in the initial shot
        character.PlantFlower(Checkpoint.FromState(nextState));
    }
}

}