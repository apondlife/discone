using Musicker;
using ThirdPerson;
using UnityAtoms.BaseAtoms;
using UnityEngine;


[RequireComponent(typeof(FMODUnity.StudioEventEmitter))]
public sealed class SimpleCharacterMusic: CharacterMusicBase {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the time interval between steps")]
    [SerializeField] float m_StepInterval = 0.2f;

    // -- refs --
    [Header("refs")]
    [Tooltip("the fmod event emitter for steps")]
    [SerializeField] FMODUnity.StudioEventEmitter m_JumpEmitter;
    [Tooltip("the fmod event emitter for jumps")]
    [SerializeField] FMODUnity.StudioEventEmitter m_StepEmitter;

    // -- props --
    /// the containing DisconeCharacter
    // TODO: inject this better in the future (parent call these events)
    DisconeCharacter m_Container;

    /// the current step time
    float m_StepTime = 0.0f;

    /// the time of the next step
    float m_NextStepTime = 0.0f;

    // -- lifecycle --
    #if !UNITY_SERVER
    void Start() {
        base.Start();

        // set deps
        m_Container = GetComponentInParent<DisconeCharacter>();

        //  set events
        m_Container.Character.Events.Bind(CharacterEvent.Jump, PlayJump);
        m_Container.OnSimulationChanged += OnSimulationChanged;
    }

    void Update() {
        // update state
        Step();

        // play audio
        PlayStep();
    }
    #endif

    // -- commands --
    // update current step progress
    void Step() {
        if (!State.Next.IsOnGround) {
            return;
        }

        // copy a bunch of stuff from gpc
        float dist = StepVelocity.magnitude * Time.timeScale;
        float stride = 1.0f + dist * 0.3f; // [what is this?]
        m_StepTime += (dist / stride) * (Time.deltaTime / m_StepInterval);
    }

    // -- c/play
    /// play step audio
    void PlayStep() {
        // if we are stepping at all
        if (StepVelocity == Vector3.zero) {
            return;
        }

        // if it's time to play a step
        if (m_StepTime < m_NextStepTime) {
            return;
        }

        FMODPlayer.PlayEvent(new FMODEvent {
            emitter = m_StepEmitter,
            parameters = CurrentFmodParams
        });

        // advance step
        m_NextStepTime += 0.5f;
    }

    /// play jump audio
    void PlayJump() {
        FMODPlayer.PlayEvent(new FMODEvent(m_JumpEmitter, CurrentFmodParams));
    }

    // -- events --
    private void OnSimulationChanged(DisconeCharacter.Simulation sim)
    {
        enabled = sim != DisconeCharacter.Simulation.None;
    }

    // -- queries --
    /// the character's step (planar) velocity
    Vector3 StepVelocity {
        get => State.Next.GroundVelocity;
    }
}