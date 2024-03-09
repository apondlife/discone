using ThirdPerson;
using UnityEngine;
using FMODUnity;
using NaughtyAttributes;
#if UNITY_EDITOR
using NaughtyAttributes.Editor;
#endif

namespace Discone {

public sealed class SimpleCharacterMusic: CharacterMusicBase {
    // -- refs --
    [Header("refs")]
    [Tooltip("the fmod event for continuous character sounds")]
    [SerializeField] EventReference m_Continuous;

    [Tooltip("the fmod event for jumps")]
    [SerializeField] EventReference m_Jump;

    [Tooltip("the fmod event for steps")]
    [SerializeField] EventReference m_Step;

    [Tooltip("the fmod event for walking off ledge")]
    [SerializeField] EventReference m_WalkOffLedge;

    [Header("params")]
    [SerializeField] float cellSize = 1f;
    public enum SequenceMode {
        Sequential,
        Random
        // A 'Shuffle' mode like FMOD has might be useful too
    }
    [SerializeField] SequenceMode sequenceMode;

    StudioEventEmitter m_ContinuousEmitter;
    StudioEventEmitter m_JumpEmitter;
    StudioEventEmitter m_StepEmitter;
    StudioEventEmitter m_WalkOffLedgeEmitter;

    FMODParams _fmodParams;

    // these should probably all just be somewhere shared (charactermusicbase?)
    const string k_ParamSpeed = "Speed";  // float, 0 to ~50 (~15 for running on flat surface)
    // const string k_ParamSlope = "Slope"; // float, -1 to 1
    const string k_ParamPitch = "Pitch";   // float (semitones) -24 to 24
    const string k_ParamIsOnWall = "IsOnWall";   // bool (0 or 1)
    const string k_ParamIsOnGround = "IsOnGround";   // bool (0 or 1)
    const string k_ParamIsHittingGround = "IsHittingGround";   // bool (0 or 1)
    const string k_ParamIsHittingWall = "IsHittingWall";   // bool (0 or 1)
    const string k_ParamIsLeavingGround = "IsLeavingGround";   // bool (0 or 1)
    const string k_ParamIndex = "Index";   // int (0 to 100)

    // (This sucks, would be nice if we could get these from fmod somehow, or else figure out a different architecture)
    const int k_NStepSamples = 45;
    const int k_NJumpSamples = 52;
    const int k_NWalkOffLedgeSamples = 6;


    int stepIndex = 0;
    int jumpIndex = 0;
    int previousPositionHash = 0;

    bool _stepThisFrame = false;
    bool _jumpThisFrame = false;

    // -- lifecycle --
    #if !UNITY_SERVER
    protected override void Start() {
        base.Start();

        //  set events
        m_Container.Character.Events.Subscribe(CharacterEvent.Jump, OnJump);

        // add emitters
        m_StepEmitter = gameObject.AddComponent<StudioEventEmitter>();
        m_JumpEmitter = gameObject.AddComponent<StudioEventEmitter>();
        m_WalkOffLedgeEmitter = gameObject.AddComponent<StudioEventEmitter>();
        m_ContinuousEmitter = gameObject.AddComponent<StudioEventEmitter>();

        m_StepEmitter.EventReference = m_Step;
        m_JumpEmitter.EventReference = m_Jump;
        m_WalkOffLedgeEmitter.EventReference = m_WalkOffLedge;
        m_ContinuousEmitter.EventReference = m_Continuous;

        _fmodParams = new();
    }

    public override void OnStep(int foot, bool isRunning) {
        if (State == null) {
            return;
        }

        if (Speed < 0.01f) {
            // we get ghostly step events from the animator even when idle
            // since the walk animation is blended in at some epsilon amount
            return;
        }

        _stepThisFrame = true; // do it this way to avoid duplicating sounds for walk and run animations

        // Debug.Log($"Walk step {foot}");
    }

    public void OnJump() {
        _jumpThisFrame = true;
    }

    // Must run in fixed update for state checks to make sense!
    void FixedUpdate() {
        if (State == null) {
            return;
        }

        if (!m_ContinuousEmitter.IsPlaying()) {
            // [idk why this doesn't seem to work when called in Start()]
            m_ContinuousEmitter.Play();
        }
        if (_stepThisFrame) {
            // doing it this way might cause a single frame of latency, not sure, doesn't really matter i guess
            if (IsOnGround || IsOnWall) {
                PlayStep();
            }
            _stepThisFrame = false;
        }

        if ((IsHittingWall || IsHittingGround) && !_jumpThisFrame) {
            // Debug.Log("hitting ground");
            PlayStep(); // TODO should distinguish from step sound [maybe a muted or percussive pluck (or chord?)]
        }

        if (IsLeavingGround && !_jumpThisFrame) {
            PlayWalkOffLedge();
        }

        if (_jumpThisFrame) {
            PlayJump();
            _jumpThisFrame = false;
        }

        // Update params for continuous emitter
        UpdateFmodParams();
        m_ContinuousEmitter.SetParameters(_fmodParams);
    }
    #endif

    void PlayJump() {
        UpdatePositionHash();

        // Debug.Log($"Jump speed: {Speed}");
        UpdateFmodParams();
        _fmodParams[k_ParamPitch] = 0f;
        _fmodParams[k_ParamIndex] = MakeIndex(jumpIndex, k_NJumpSamples);
        FMODPlayer.PlayEvent(new FMODEvent(m_JumpEmitter, _fmodParams));
        jumpIndex++;
    }

    void PlayStep() {
        UpdatePositionHash();

        UpdateFmodParams();
        _fmodParams[k_ParamPitch] = SlopeToPitch(VelocitySlope);
        _fmodParams[k_ParamIndex] = MakeIndex(stepIndex, k_NStepSamples);
        // Debug.Log($"step index: {ps[k_ParamIndex]}");
        FMODPlayer.PlayEvent(new FMODEvent(m_StepEmitter, _fmodParams));
        stepIndex++;
    }

    void PlayWalkOffLedge() {
        UpdatePositionHash();

        UpdateFmodParams();
        _fmodParams[k_ParamPitch] = SlopeToPitch(SurfaceSlope);
        _fmodParams[k_ParamIndex] = MakeIndex(stepIndex, k_NWalkOffLedgeSamples);

        FMODPlayer.PlayEvent(new FMODEvent(m_WalkOffLedgeEmitter, _fmodParams));
        stepIndex++;
    }

    void UpdateFmodParams() {
        // _fmodParams[k_ParamSlope]           = Slope;
        _fmodParams[k_ParamSpeed]           = Speed;
        _fmodParams[k_ParamIsOnGround]      = IsOnGround      ? 1f : 0f;
        _fmodParams[k_ParamIsOnWall]        = IsOnWall        ? 1f : 0f;
        _fmodParams[k_ParamIsHittingWall]   = IsHittingWall   ? 1f : 0f;
        _fmodParams[k_ParamIsHittingGround] = IsHittingGround ? 1f : 0f;
        _fmodParams[k_ParamIsLeavingGround] = IsLeavingGround ? 1f : 0f;
    }

    int MakeIndex(int subIndex, int sampleCount) {
        if (sequenceMode == SequenceMode.Random) {
            // rather than a direct sequence (following fmod), play a random sequence determined by the position hash
            System.Random rand = new(subIndex);
            // Debug.Log($"pre-twist subIndex: {subIndex}");
            subIndex = Mathf.Abs(rand.Next());
        }

        return (int)(((long)PositionHash() + subIndex) % sampleCount);
    }

    void UpdatePositionHash() {
        // if jumping/stepping in a different grid cell to last time, reset the counters
        // [should probably happen separately for jump/step but whatever]
        int positionHash = PositionHash();
        if (previousPositionHash != positionHash) {
            stepIndex = jumpIndex = 0;
            previousPositionHash = positionHash;
        }
    }
    int PositionHash() {
        // round position to cellSize
        Vector3 pos = transform.position;
        // pos.y = 0f; // ignore vertical for now
        Vector3Int gridToPos = Vector3Int.FloorToInt(pos / cellSize);
        return Mathf.Abs(gridToPos.GetHashCode());
    }

    float SlopeToPitch(float slope) {
        int[] pitches = { -7, -7, -7, -7, -5, -5, 0, 2, 4, 5, 7, 7, 7 };
        int i = (int)(Mathf.InverseLerp(-1f, 1f, slope) * pitches.Length);
        return (float)pitches[i];
    }

    // -- queries --
    [ShowNativeProperty]
    float VelocitySlope {
        get => State.Curr.Velocity.normalized.y; // -1 to 1
    }

    [ShowNativeProperty]
    float SurfaceSlope {
        get => State.Curr.MainSurface.Angle/180f; // 0 to 1?
    }

    [ShowNativeProperty]
    float Speed {
        get => State.Curr.Velocity.magnitude;
    }

    [ShowNativeProperty]
    bool IsOnGround {
        get => State.Curr.IsOnGround;
    }

    [ShowNativeProperty]
    bool IsOnWall {
        get => State.Curr.IsOnWall;
    }

    [ShowNativeProperty]
    bool IsHittingWall {
        get => !State.Curr.IsOnWall && State.Next.IsOnWall;
    }

    [ShowNativeProperty]
    bool IsHittingGround {
        get => !State.Curr.IsOnGround && State.Next.IsOnGround;
    }

    [ShowNativeProperty]
    bool IsLeavingGround {
        get => State.Curr.IsOnGround && !State.Next.IsOnGround;
    }
}

// Force redraw of exposed native properties in inspector every frame
#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(SimpleCharacterMusic))]
sealed class Editor: NaughtyInspector {
    public override bool RequiresConstantRepaint() => true;
}
#endif

}