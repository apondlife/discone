using Soil;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Discone {

// TODO: rename to IntroSequence or RootSequence? IntroSequence, DreamSequence, IslandSequence
sealed class GameSequence: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the current step")]
    [NaughtyAttributes.ReadOnly]
    [SerializeField] GameStep m_Step;

    [Tooltip("all the game triggers in no particular order")]
    [SerializeField] GameStepTrigger[] m_Triggers;

    // -- dispatched --
    [Header("dispatched")]
    [Tooltip("when this sequence starts")]
    [SerializeField] VoidEvent m_Started;

    [Tooltip("when a game step starts")]
    [SerializeField] GameStepEvent m_GameStep_Started;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("advance to the next game step")]
    [SerializeField] VoidEvent m_Advance;

    // -- refs --
    [Header("refs")]
    [Tooltip("the current character")]
    [SerializeField] DisconeCharacterVariable m_CurrentCharacter;

    [Tooltip("the shared data store")]
    [SerializeField] Store m_Store;

    // -- fields --
    /// if exiting the current step, but haven't received an enter
    bool m_IsExiting;

    /// a set of event subscriptions
    DisposeBag m_Subscriptions = new();

    // -- lifecycle --
    void Start() {
        // init steps
        foreach (var trigger in m_Triggers) {
            trigger.OnEnter(OnStepEnter);
            trigger.OnExit(OnStepExit);
        }

        // bind events
        m_Subscriptions
            .Add(m_Advance, OnAdvance)
            .Add(m_Store.LoadFinished, OnLoadFinished)
            // TODO: a once subscription or initial character event
            .Add(m_CurrentCharacter.ChangedWithHistory, OnCharacterChanged);
    }

    void Update() {
        if (m_IsExiting) {
            Finish();
        }
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// start the sequence
    void Init() {
        m_Started.Raise();

        // start the initial step
        var initialStep = FindInitialStep();
        Log.Intro.W($"start initial step {initialStep}");

        m_Step = initialStep;
        m_GameStep_Started.Raise(initialStep);
    }

    /// start a new step
    void StartStep(GameStep step) {
        if (step <= m_Step) {
            return;
        }

        Log.Intro.I($"start step {step}");
        m_Step = step;
        m_IsExiting = false;
        m_GameStep_Started.Raise(step);
    }

    /// finish a step, if possible
    void FinishStep(GameStep step) {
        if (step == m_Step) {
            m_IsExiting = true;
        }
    }

    /// finish the game sequence
    void Finish() {
        StartStep(GameStep.Finished);
        Destroy(this);
    }

    // -- queries --
    /// find the initial step based on the character's current position
    GameStep FindInitialStep() {
        var pos = m_CurrentCharacter.Value.State.Position;

        foreach (var trigger in m_Triggers) {
            if (trigger.Contains(pos)) {
                return trigger.Step;
            }
        }

        return GameStep.Finished;
    }

    // -- events --
    /// when an advance is requested
    void OnAdvance() {
        StartStep(m_Step + 1);
    }

    /// when the store finishes loading data
    void OnLoadFinished() {
        if (m_Store.Player.HasData) {
            Finish();
        }
    }

    /// when the store finishes loading data
    void OnCharacterChanged(DisconeCharacterPair character) {
        if (!character.Item2) {
            Init();
        }
    }

    /// when we exit a step trigger
    void OnStepExit(GameStep step) {
        FinishStep(step);
    }

    /// when we enter a step trigger
    void OnStepEnter(GameStep step) {
        Log.Intro.W($"got trigger for {step}");
        StartStep(step);
    }
}

}