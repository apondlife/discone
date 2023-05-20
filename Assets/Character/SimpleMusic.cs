using Musicker;
using ThirdPerson;
using UnityAtoms.BaseAtoms;
using UnityEngine;


// [copy pasted a lot from CharacterMusic & FmodMusicSource, need to refactor]

[RequireComponent(typeof(FMODUnity.StudioEventEmitter))]
public sealed class SimpleMusic: CharacterMusicBase {
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

    [Tooltip("if running as a standalone server")]
    [SerializeField] BoolReference m_IsStandalone;

    // -- props --
    /// the containing DisconeCharacter
    // TODO: inject this better in the future (parent call these events)
    DisconeCharacter m_Container;

    /// the current step time
    float m_StepTime = 0.0f;

    /// the time of the next step
    float m_NextStepTime = 0.0f;

    const string k_ParamGrounded = "IsGrounded";

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
        if (m_IsStandalone) {
            return;
        }

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
        // if were stepping at all
        if (StepVelocity == Vector3.zero) {
            return;
        }

        // if it's time to play a step
        if (m_StepTime < m_NextStepTime) {
            return;
        }

        PlayEvent(m_StepEmitter, null);

        // advance step
        m_NextStepTime += 0.5f;
    }

    /// play jump audio
    void PlayJump() {
        PlayEvent(m_JumpEmitter, null);
    }

    // -- events --
    private void OnSimulationChanged(DisconeCharacter.Simulation sim)
    {
        enabled = sim != DisconeCharacter.Simulation.None;
    }

    private FmodMusicSource.FmodParams GetFmodParams() {
        return new FmodMusicSource.FmodParams {
            isGrounded = IsGrounded
        };
    }

    // -- fmod -- 
    // [Should probably be in a separate reusable component]
    void PlayEvent(FMODUnity.StudioEventEmitter emitter, FmodMusicSource.FmodParams? fmodParams) {
        // play the event for this note
        if (emitter) emitter.Play();
        
        if (fmodParams.HasValue) {
            emitter.SetParameter(k_ParamGrounded, fmodParams.Value.isGrounded ? 1f : 0f);
            // [and so on..]
        }
    }

    // -- queries --
    /// the character's step (planar) velocity
    Vector3 StepVelocity {
        get => State.Next.GroundVelocity;
    }
    /// whether or not the character is grounded
    bool IsGrounded {
        get => State.Next.IsOnGround;
    }

    ThirdPerson.CharacterState State {
        get => m_Container.Character.State;
    }
}