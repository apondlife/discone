using UnityAtoms;
using UnityEngine;

namespace Discone {

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
    [Tooltip("when a game step starts")]
    [SerializeField] GameStepEvent m_GameStep_Started;

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
        Log.Game.I($"start step {m_Step}");

        // init steps
        foreach (var trigger in m_Triggers) {
            trigger.OnEnter(OnStepEnter);
            trigger.OnExit(OnStepExit);
        }

        // add subscriptions
        m_Subscriptions
            .Add(m_Store.LoadFinished, OnLoadFinished)
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
    /// start a new step
    void StartStep(GameStep step) {
        if (step <= m_Step) {
            return;
        }

        Log.Game.I($"start step {step}");
        m_Step = step;
        m_IsExiting = false;

        if (step == GameStep.Finished || IsReady) {
            m_GameStep_Started.Raise(step);
        }
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
    /// if the sequence is ready to start
    bool IsReady {
        get => m_CurrentCharacter;
    }

    // -- events --
    /// when the store finishes loading data
    void OnLoadFinished() {
        if (m_Store.Player.HasData) {
            Finish();
        }
    }

    /// when the store finishes loading data
    void OnCharacterChanged(DisconeCharacterPair character) {
        if (!character.Item2) {
            m_GameStep_Started.Raise(m_Step);
        }
    }

    /// when we exit a step trigger
    void OnStepExit(GameStep step) {
        FinishStep(step);
    }

    /// when we enter a step trigger
    void OnStepEnter(GameStep step) {
        StartStep(step);
    }
}

}