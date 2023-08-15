using ThirdPerson;
using UnityEngine;
using FMODUnity;
using NaughtyAttributes;

public sealed class SimpleCharacterMusic: CharacterMusicBase {
    // -- refs --
    [Header("refs")]
    [Tooltip("the fmod event for continuous character sounds")]
    [SerializeField] EventReference m_Continuous;

    [Tooltip("the fmod event for jumps")]
    [SerializeField] EventReference m_Jump;

    [Tooltip("the fmod event for steps")]
    [SerializeField] EventReference m_Step;

    [Header("params")]
    [SerializeField] float cellSize = 1f;

    StudioEventEmitter m_ContinuousEmitter;
    StudioEventEmitter m_JumpEmitter;
    StudioEventEmitter m_StepEmitter;

    // these should probably all just be somewhere shared (charactermusicbase?)
    const string k_ParamSpeed= "Speed";  // float, 0 to ~50 (~15 for running on flat surface)
    const string k_ParamSlope = "Slope"; // float, -1 to 1
    const string k_ParamPitch = "Pitch";   // float (semitones) -24 to 24
    const string k_ParamIsOnWall = "IsOnWall";   // bool (0 or 1)
    const string k_ParamIsOnGround = "IsOnGround";   // bool (0 or 1)
    const string k_ParamIndex = "Index";   // int (0 to 100)

    int stepIndex = 0;
    int jumpIndex = 0;
    int previousPositionHash = 0;

    bool _stepThisFrame = false;

    // -- lifecycle --
    #if !UNITY_SERVER
    protected override void Start() {
        base.Start();

        // set deps
        m_Container = GetComponentInParent<DisconeCharacter>();

        //  set events
        m_Container.Character.Events.Bind(CharacterEvent.Jump, PlayJump);

        // add emitters
        m_StepEmitter = gameObject.AddComponent<StudioEventEmitter>();
        m_JumpEmitter = gameObject.AddComponent<StudioEventEmitter>();
        m_ContinuousEmitter = gameObject.AddComponent<StudioEventEmitter>();

        // TODO: is it a bug that this game object might be enabled and disabled?
        m_ContinuousEmitter.Play();

        m_StepEmitter.EventReference = m_Step;
        m_JumpEmitter.EventReference = m_Jump;
        m_ContinuousEmitter.EventReference = m_Continuous;
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
            // doing it this way might cause a single frame of latency, not sure, doesn't really matter i guess
            if (IsOnGround || IsOnWall) {
                PlayStep();
            }
            _stepThisFrame = false;
        }

        // if moving into a new grid cell, reset the counter
        int positionHash = PositionHash();
        if (previousPositionHash != positionHash) {
            stepIndex = jumpIndex = 0;
            previousPositionHash = positionHash;
        }

        // Update params for continuous emitter
        m_ContinuousEmitter.SetParameters(CurrentFmodParams);
    }
    #endif

    /// play jump audio
    void PlayJump() {
        // Debug.Log($"Jump speed: {Speed}");
        FMODParams ps = CurrentFmodParams;
        ps[k_ParamIndex] = (jumpIndex + PositionHash())%52;
        FMODPlayer.PlayEvent(new FMODEvent(m_JumpEmitter, ps));
        jumpIndex++;
    }

    void PlayStep() {
        // do pitch quantization here because it's much harder to do in fmod
        int[] pitches = {-7, -7, -7, -7, -5, -5, 0, 2, 4, 5, 7, 7, 7};
        int i = (int)(Mathf.InverseLerp(-1f, 1f, Slope)*pitches.Length);
        float pitch = (float)pitches[i];
        // Debug.Log(pitch);
        FMODParams ps = CurrentFmodParams;
        ps[k_ParamPitch] = pitch;
        ps[k_ParamIndex] = (stepIndex + PositionHash())%52;
        FMODPlayer.PlayEvent(new FMODEvent (m_StepEmitter, ps));
        stepIndex++;
    }

    protected override FMODParams CurrentFmodParams => new FMODParams {
        [k_ParamSlope] = Slope,
        [k_ParamSpeed] = Speed,
        [k_ParamIsOnGround] = IsOnGround ? 1f : 0f,
        [k_ParamIsOnWall] = IsOnWall ? 1f : 0f,
        // [k_ParamIndex] = Index
    };

    int PositionHash() {
        // round position to cellSize
        Vector3 pos = transform.position;
        pos.y = 0f; // ignore vertical for now
        Vector3Int gridToPos = Vector3Int.FloorToInt(pos/cellSize);
        return Mathf.Abs(gridToPos.GetHashCode());
    }

    // -- queries --
    // slope (-1 to 1) of current velocity
    // [ShowNativeProperty]
    // float Index {
    //     // get => soundIndex%52; // TODO figure out a better way of looping the index in fmod
    //     get => PositionHash()%52;
    // }
    [ShowNativeProperty]
    float Slope {
        get => State.Next.Velocity.normalized.y;
    }

    [ShowNativeProperty]
    float Speed {
        get => State.Next.Velocity.magnitude;
    }

    [ShowNativeProperty]
    bool IsOnGround {
        get => State.Next.IsOnGround;
    }

    [ShowNativeProperty]
    bool IsOnWall {
        get => State.Next.IsOnWall;
    }
}