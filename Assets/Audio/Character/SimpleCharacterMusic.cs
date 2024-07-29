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
    [SerializeField] int m_NJumpSamples; // Really annoying having to set these manually - would be great if we could get these from FMOD somehow

    [Tooltip("the fmod event for steps")]
    [SerializeField] EventReference m_Step;
    [SerializeField] int m_NStepSamples;

    [Tooltip("the fmod event for walking off ledge")]
    [SerializeField] EventReference m_WalkOffLedge;
    [SerializeField] int m_NWalkOffLedgeSamples;

    [Header("params")]
    [SerializeField] float cellSize = 1f;
    public enum SequenceMode {
        Sequential,
        Random
        // A 'Shuffle' mode like FMOD has might be useful too
    }
    [SerializeField] SequenceMode sequenceMode;
    [Tooltip("Whether or not to stop jump sound when landing")]
    [SerializeField] bool chokeJump;
    [Tooltip("Whether or not to stop walk-off-ledge sound when landing")]
    [SerializeField] bool chokeWalkOffLedge;

    StudioEventEmitter m_ContinuousEmitter;
    StudioEventEmitter m_JumpEmitter;
    StudioEventEmitter m_StepEmitter;
    StudioEventEmitter m_WalkOffLedgeEmitter;

    FMODParams _fmodParams;

    // these should probably all just be somewhere shared (charactermusicbase?)
    const string k_ParamSpeedSquared = "Speed";  // float, 0 to ~2500 (~225 for running on flat surface)
    // const string k_ParamSlope = "Slope"; // float, -1 to 1

    const string k_ParamDeltaSpeedSquared = "DeltaSpeed"; // float, 0 to ? (~360 for a big jump)

    const string k_ParamPitch = "Pitch";   // float (semitones) -24 to 24
    const string k_ParamIsOnWall = "IsOnWall";   // bool (0 or 1)
    const string k_ParamIsOnGround = "IsOnGround";   // bool (0 or 1)
    const string k_ParamIsHittingGround = "IsHittingGround";   // bool (0 or 1)
    const string k_ParamIsHittingWall = "IsHittingWall";   // bool (0 or 1)
    const string k_ParamIsLeavingGround = "IsLeavingGround";   // bool (0 or 1)
    const string k_ParamIndex = "Index";   // int (0 to 100)

    int stepIndex = 0;
    int jumpIndex = 0;
    int previousPositionHash = 0;

    // -- lifecycle --
    #if !UNITY_SERVER
    protected override void Start() {
        base.Start();

        // add emitters
        m_StepEmitter = gameObject.AddComponent<StudioEventEmitter>();
        m_JumpEmitter = gameObject.AddComponent<StudioEventEmitter>();
        m_WalkOffLedgeEmitter = gameObject.AddComponent<StudioEventEmitter>();
        m_ContinuousEmitter = gameObject.AddComponent<StudioEventEmitter>();

        m_StepEmitter.EventReference = m_Step;
        m_JumpEmitter.EventReference = m_Jump;
        m_WalkOffLedgeEmitter.EventReference = m_WalkOffLedge;
        m_ContinuousEmitter.EventReference = m_Continuous;

        // Preload samples so that there's less delay when playing them
        foreach(var emitter in new [] {m_StepEmitter, m_JumpEmitter, m_WalkOffLedgeEmitter, m_ContinuousEmitter} ) {
            emitter.Preload = true;
        }

        _fmodParams = new();
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

        if (IsLanding) {
            PlayLand();
        }

        if (IsStepping && !IsLanding) {
            PlayStep();
        }

        if (IsCrouched) {
            PlayStep(); // This happened by accident before but sounds kind of cool like tremolo picking
            // Leaving it in for a little while
            // TODO: what kind of guitar sound can work for crouching..?
        }
        
        if (IsJumping && State.Curr.IsOnGround) { // Don't play sound for midair jumps
            PlayJump();
        }

        if (IsLeavingGround && !IsJumping) {
            PlayWalkOffLedge();
        }

        // Update params for continuous emitter
        UpdateFmodParams();
        m_ContinuousEmitter.SetParameters(_fmodParams);
    }
    #endif

    void PlayJump() {
        UpdatePositionHash();

        UpdateFmodParams();
        // Debug.Log($"Jump: dv = {DeltaSpeedSquared}");
        _fmodParams[k_ParamPitch] = 0f;
        _fmodParams[k_ParamIndex] = MakeIndex(jumpIndex, m_NJumpSamples);
        // TODO this should probably use the jump impulse, not the overall character velocity

        FMODPlayer.PlayEvent(new FMODEvent(m_JumpEmitter, _fmodParams));
        jumpIndex++;
    }

    void PlayLand() {
        // Fadeout/release times are set in AHDSR modulator in FMOD
        if (chokeJump) {
            // TODO: some kind of choke sound here would be good probably
            m_JumpEmitter.Stop();
        }
        if (chokeWalkOffLedge) {
            m_WalkOffLedgeEmitter.Stop();
        }

        PlayStep(); // TODO some different sound here [maybe a muted or percussive pluck, or slap strings?]
    }

    void PlayStep() {
        UpdatePositionHash();

        UpdateFmodParams();
        _fmodParams[k_ParamPitch] = SlopeToPitch(VelocitySlope);
        _fmodParams[k_ParamIndex] = MakeIndex(stepIndex, m_NStepSamples);

        FMODPlayer.PlayEvent(new FMODEvent(m_StepEmitter, _fmodParams));
        stepIndex++;
    }

    void PlayWalkOffLedge() {
        UpdatePositionHash();

        UpdateFmodParams();
        _fmodParams[k_ParamPitch] = SlopeToPitch(SurfaceSlope);
        _fmodParams[k_ParamIndex] = MakeIndex(stepIndex, m_NWalkOffLedgeSamples);

        FMODPlayer.PlayEvent(new FMODEvent(m_WalkOffLedgeEmitter, _fmodParams));
        stepIndex++;
    }

    void UpdateFmodParams() {
        // _fmodParams[k_ParamSlope]           = Slope;
        _fmodParams[k_ParamSpeedSquared]      = SpeedSquared;
        _fmodParams[k_ParamDeltaSpeedSquared] = DeltaSpeedSquared;
        _fmodParams[k_ParamIsOnGround]        = IsOnGround      ? 1f : 0f;
        _fmodParams[k_ParamIsOnWall]          = IsOnWall        ? 1f : 0f;
        _fmodParams[k_ParamIsHittingWall]     = IsHittingWall   ? 1f : 0f;
        _fmodParams[k_ParamIsHittingGround]   = IsHittingGround ? 1f : 0f;
        _fmodParams[k_ParamIsLeavingGround]   = IsLeavingGround ? 1f : 0f;
    }

    int MakeIndex(int subIndex, int sampleCount) {
        if (sequenceMode == SequenceMode.Random) {
            // rather than a direct sequence (following fmod), play a random sequence determined by the position hash
            System.Random rand = new(subIndex);
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
    bool IsStepping {
        get => State.Next.Events.Contains(CharacterEvent.Step);
    }

    [ShowNativeProperty]
    bool IsLanding {
        get => State.Next.Events.Contains(CharacterEvent.Land);
    }
    
    [ShowNativeProperty]
    bool IsJumping {
        get => State.Next.Events.Contains(CharacterEvent.Jump);
    }
        
    [ShowNativeProperty]
    bool IsCrouched {
        get => State.Next.IsCrouching;
    }


    [ShowNativeProperty]
    float VelocitySlope {
        get => State.Next.Velocity.normalized.y; // -1 to 1
    }

    [ShowNativeProperty]
    float SurfaceSlope {
        get => State.Next.MainSurface.Angle/180f; // 0 to 1?
    }

    [ShowNativeProperty]
    float SpeedSquared {
        get => State.Next.Velocity.sqrMagnitude;
    }

    float DeltaSpeedSquared {
        get => (State.Next.Velocity - State.Curr.Velocity).sqrMagnitude;
    }

    [ShowNativeProperty]
    bool IsOnGround {
        get => State.Next.IsOnGround;
    }

    [ShowNativeProperty]
    bool IsOnWall {
        get => State.Next.IsOnWall;
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