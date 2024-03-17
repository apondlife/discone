using Soil;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Serialization;
using Yarn.Unity;

namespace Discone {

sealed class IntroSequence: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [FormerlySerializedAs("m_InitialTransform")]
    [Tooltip("the character's rotation reference for the initial shot")]
    [SerializeField] Transform m_StartTransform;

    [FormerlySerializedAs("m_IntroCamera")]
    [Tooltip("the intro camera")]
    [SerializeField] GameObject m_LetterCamera;

    [FormerlySerializedAs("m_IntroCameraRetrigger")]
    [Tooltip("the intro camera when returning to the letter")]
    [SerializeField] GameObject m_LetterCameraRevisit;

    // -- mechanic --
    [Header("mechanic")]
    [Tooltip("the mechanic yarn project")]
    [SerializeField] YarnProject m_Mechanic;

    [Tooltip("the mechanic node to play on eyes open")]
    [YarnNode(nameof(m_Mechanic))]
    [SerializeField] string m_Mechanic_EndNode;

    [FormerlySerializedAs("m_Mechanic_JumpToNode")]
    [Tooltip("jump to a new mechanic node")]
    [SerializeField] StringEvent m_Mechanic_Jump;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("when the dream is over")]
    [SerializeField] VoidEvent m_DreamEnded;

    [Tooltip("when a game step starts")]
    [SerializeField] GameStepEvent m_GameStep_Started;

    // -- refs --
    [Header("refs")]
    [Tooltip("the player's character")]
    [SerializeField] DisconeCharacterVariable m_CurrentCharacter;

    // -- props --
    /// if the input was performed
    bool m_DidJumpToNode;

    /// a set of event subscriptions
    readonly DisposeBag m_Subscriptions = new();

    // -- lifecycle --
    void Start() {
        // bind events
        m_Subscriptions
           .Add(m_DreamEnded, OnDreamEnded)
           .Add(m_GameStep_Started, OnGameStepStarted);
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// warp to the intro
    void Warp() {
        // towards a different direction then ice creams orientation
        var character = m_CurrentCharacter.Value;

        // set initial character state
        var nextState = character.State.Curr.Copy();
        nextState.Position = m_StartTransform.position;
        nextState.Forward = m_StartTransform.forward;
        character.ForceState(nextState);
    }

    /// start the sequence
    void Init() {
        // switch to the intro camera
        m_LetterCamera.SetActive(true);

        // plant the birthplace flower
        // TODO: plant flower somewhere in the initial shot
        var character = m_CurrentCharacter.Value;
        character.PlantFlower(Checkpoint.FromState(character.CurrentState));
    }

    /// finish the sequence and destroy it
    void Finish() {
        // blend to the game camera
        m_LetterCamera.SetActive(false);

        // enable revisiting this camera
        m_LetterCameraRevisit.SetActive(true);

        // ...
        Destroy(this);
    }

    // -- events --
    /// when the dream sequence ends
    void OnDreamEnded() {
        Warp();
    }

    /// when a new game step starts
    void OnGameStepStarted(GameStep step) {
        if (step == GameStep.Intro) {
            Init();
        } else if (step > GameStep.Intro) {
            Finish();
        }
    }
}

}