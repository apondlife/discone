using Musicker;
using ThirdPerson;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using FMODUnity;

public sealed class SimpleCharacterMusic: CharacterMusicBase {
    // -- refs --
    [Header("refs")]
    [Tooltip("the fmod event for continuous character sounds")]
    [SerializeField] EventReference m_Continuous;
    [Tooltip("the fmod event for jumps")]
    [SerializeField] EventReference m_Jump;
    [Tooltip("the fmod event for steps")]
    [SerializeField] EventReference m_Step;

    StudioEventEmitter m_ContinuousEmitter;
    StudioEventEmitter m_JumpEmitter;
    StudioEventEmitter m_StepEmitter;

    // these should probably all just be somewhere shared (charactermusicbase?)
    static readonly string k_ParamSpeed= "Speed";  // float, 0 to ~20
    static readonly string k_ParamSlope = "Slope"; // float, -1 to 1
    static readonly string k_ParamPitch = "Pitch";   // float (semitones) -24 to 24

    bool _stepThisFrame = false;

    // -- lifecycle --
    #if !UNITY_SERVER
    protected override void Start() {
        base.Start();

        // set deps
        m_Container = GetComponentInParent<DisconeCharacter>();

        //  set events
        m_Container.Character.Events.Bind(CharacterEvent.Jump, PlayJump);
        m_Container.OnSimulationChanged += OnSimulationChanged;

        // add emitters
        m_StepEmitter = gameObject.AddComponent<StudioEventEmitter>();
        m_StepEmitter.EventReference = m_Step;
        m_JumpEmitter = gameObject.AddComponent<StudioEventEmitter>();
        m_JumpEmitter.EventReference = m_Jump;
        m_ContinuousEmitter = gameObject.AddComponent<StudioEventEmitter>();
        m_ContinuousEmitter.EventReference = m_Continuous;
        m_ContinuousEmitter.Play();
    }
    public override void OnStep(int foot, bool isRunning) {
        if (Speed < 0.01f) {
            // we get ghostly step events from the animator even when idle
            // since the walk animation is blended in at some epsilon amount
            return;
        }

        _stepThisFrame = true; // do it this way to avoid duplicating sounds for walk and run animations

        // Debug.Log($"Walk step {foot}");
    }

    void Update() {
        if (_stepThisFrame) {
            // this might cause a single frame of latency, not sure, doesn't really matter i guess
            if (IsOnGround) {
                PlayStep();
            }
            _stepThisFrame = false;
        }

        // Update params for continuous emitter
        m_ContinuousEmitter.SetParameters(CurrentFmodParams);
    }
    #endif

    /// play jump audio
    void PlayJump() {
        FMODPlayer.PlayEvent(new FMODEvent(m_JumpEmitter, CurrentFmodParams));
    }

    void PlayStep() {
        // do pitch quantization here because it's much harder to do in fmod
        int[] pitches = {-7, -7, -7, -7, -5, -5, 0, 2, 4, 5, 7, 7, 7};
        int i = (int)(Mathf.InverseLerp(-1f, 1f, Slope)*pitches.Length);
        float pitch = (float)pitches[i];
        // Debug.Log(pitch);
        FMODParams ps = CurrentFmodParams;
        Debug.Log(pitch);
        ps[k_ParamPitch] = pitch;
        FMODPlayer.PlayEvent(new FMODEvent (m_StepEmitter, ps));
    }

    protected override FMODParams CurrentFmodParams => new FMODParams {
            [k_ParamSlope] = Slope,
            [k_ParamSpeed] = Speed,
            [k_ParamGrounded] = IsOnGround ? 1f : 0f
    };

    // -- events --
    private void OnSimulationChanged(DisconeCharacter.Simulation sim)
    {
        enabled = sim != DisconeCharacter.Simulation.None;
    }

    // -- queries --
    // slope (-1 to 1) of current velocity
    float Slope {
        get => State.Next.Velocity.normalized.y;
    }
    float Speed {
        get => State.Next.Velocity.magnitude;
    }
    bool IsOnGround {
        get => State.Next.IsOnGround;
    }
}