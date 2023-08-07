using Musicker;
using ThirdPerson;
using UnityAtoms.BaseAtoms;
using UnityEngine;


[RequireComponent(typeof(FMODUnity.StudioEventEmitter))]
public sealed class SimpleCharacterMusic: CharacterMusicBase {
    // -- refs --
    [Header("refs")]
    [Tooltip("the fmod event emitter for steps")]
    [SerializeField] FMODUnity.StudioEventEmitter m_JumpEmitter;
    [Tooltip("the fmod event emitter for jumps")]
    [SerializeField] FMODUnity.StudioEventEmitter m_StepEmitter;

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
    }
    public override void OnStep(int foot, bool isRunning) {
        if (Speed < 0.01f) {
            // we get ghostly step events from the animator even when idle
            // since the walk animation is blended in at some epsilon amount
            return;
        }

        _stepThisFrame = true; // do it this way to avoid duplicating sounds for walk and run animations

        // Debug.Log($"Walk step {foot}");
        // if (IsOnGround) {
        //     PlayStep();
        // }
    }

    void Update() {
        if (_stepThisFrame) {
            // this might cause a single frame of latency, not sure, doesn't really matter i guess
            if (IsOnGround) {
                PlayStep();
            }
            _stepThisFrame = false;
        }
    }
    #endif

    /// play jump audio
    void PlayJump() {
        FMODPlayer.PlayEvent(new FMODEvent(m_JumpEmitter, new FMODParams {
            [k_ParamPitch] = 0f
        }));
    }

    void PlayStep() {
        // do pitch quantization here because it's much harder to do in fmod
        int[] pitches = {-5, -3, 0, 2, 5};
        int i = (int)(Mathf.InverseLerp(-1f, 1f, Slope)*pitches.Length);
        float pitch = (float)pitches[i];
        // Debug.Log(pitch);
        FMODPlayer.PlayEvent(new FMODEvent (m_StepEmitter, new FMODParams {
            [k_ParamPitch] = pitch
        }));
    }

    // protected override FMODParams CurrentFmodParams {
    //     get {
    //         FMODParams b = base.CurrentFmodParams;
    //         b[k_ParamSpeed] = Speed;
    //         b[k_ParamSlope] = Slope;
    //         return b;
    //     }
    // }

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