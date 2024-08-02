using ThirdPerson;
using UnityEngine;
using FMODUnity;
using System.Linq;
using NaughtyAttributes;
using System;

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
    [SerializeField] EventReference m_WalkOffLedge; // TODO: Why does this one have such huge delay?? So weird
    [SerializeField] int m_NWalkOffLedgeSamples;
    
    [Tooltip("the fmod event for crouching")]
    [SerializeField] EventReference m_Crouch;

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
    StudioEventEmitter m_CrouchEmitter;

    FMODParams _fmodParams;

    const float k_RecentlyLandedLookbackSecs = 0.05f;
    const float k_StarterIslandBaseY = -1783f; // Should probably be configured elsewhere but whatever

    // these should probably all just be somewhere shared (charactermusicbase?)
    const string k_ParamSpeedSquared = "Speed";  // float, 0 to ~2500 (~225 for running on flat surface)
    // const string k_ParamSlope = "Slope"; // float, -1 to 1

    const string k_ParamDeltaSpeedSquared = "DeltaSpeed"; // float, 0 to ? (~360 for a big jump)

    const string k_ParamGain = "Gain";   // float 0 to 1, maps to +0 to +10dB
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

    float mostRecentLandedTime = float.MinValue;

    // -- lifecycle --
    #if !UNITY_SERVER
    protected override void Start() {
        base.Start();

        // add emitters
        m_StepEmitter = gameObject.AddComponent<StudioEventEmitter>();
        m_JumpEmitter = gameObject.AddComponent<StudioEventEmitter>();
        m_WalkOffLedgeEmitter = gameObject.AddComponent<StudioEventEmitter>();
        m_ContinuousEmitter = gameObject.AddComponent<StudioEventEmitter>();
        m_CrouchEmitter = gameObject.AddComponent<StudioEventEmitter>();

        m_StepEmitter.EventReference = m_Step;
        m_JumpEmitter.EventReference = m_Jump;
        m_WalkOffLedgeEmitter.EventReference = m_WalkOffLedge;
        m_ContinuousEmitter.EventReference = m_Continuous;
        m_CrouchEmitter.EventReference = m_Crouch;

        // Preload samples so that there's less delay when playing them
        foreach(var emitter in new [] {m_StepEmitter, m_JumpEmitter, m_WalkOffLedgeEmitter, m_CrouchEmitter, m_ContinuousEmitter} ) {
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

        if (IsLanding && !IsOnWall) {
            mostRecentLandedTime = Time.time;
            PlayLand();
        }

        if (IsStepping && !RecentlyLanded) { // Try to avoid cluster of footstep sounds after landing
            PlayStep();
        }

        if (IsCrouching) { // as in, is going into a crouch this frame
            PlayCrouch();
        }
        if (IsStoppingCrouch) {
            StopCrouch();
        }
        
        if (IsJumping && State.Curr.IsOnGround) { // Don't play sound for midair jumps
            PlayJump();
        }

        if (IsLeavingGround && !IsJumping) {
            PlayWalkOffLedge();
        }

        // PlayStep();

        // Update params for continuous emitter
        UpdateFmodParams();
        m_ContinuousEmitter.SetParameters(_fmodParams);
    }
    #endif

    void PlayJump() {
        UpdatePositionHash();

        UpdateFmodParams();
        // Debug.Log($"Jump: dv = {DeltaSpeedSquared}");
        _fmodParams[k_ParamPitch] = MakePitch() % 7; // Wrap to center fifth (?)
        _fmodParams[k_ParamIndex] = MakeIndex(jumpIndex, m_NJumpSamples);
        // TODO this should probably use the jump impulse, not the overall character velocity

        FMODPlayer.PlayEvent(new FMODEvent(m_JumpEmitter, _fmodParams));
        jumpIndex++;
    }

    void PlayLand() {
        // Fadeout/release times are set in AHDSR modulator in FMOD
        if (chokeJump) {
            // Jump event plays its own choke sound
            m_JumpEmitter.Stop();
        }
        if (chokeWalkOffLedge) {
            m_WalkOffLedgeEmitter.Stop();
        }
    }

    // random noodling up and down pentatonic scale each step
    int i = 6;
    int[] pent = {-5, -3, -1, 0, 2, 4, 7, 9, 11, 12, 14, 16, 19};
    int MakePitch1() {
        int s = UnityEngine.Random.Range(0, 2)*2 -1; // step up or down

        i += s;
        i = Mathf.Clamp(i, 0, pent.Length-1);

        int k = i + (int)SlopeToPitch(VelocitySlope);
        k = Mathf.Clamp(k, 0, pent.Length-1);
        return pent[k];
    }

    [SerializeField] float scaleStepsPerAcceleration = 0.4f;
    // noodling up and down pentatonic scale based on accel/decel
    int MakePitch3() {
        int s = (int)(SpeedSquaredDelta*scaleStepsPerAcceleration);
        Debug.Log($"s = {s}");

        i += s;
        i = Mathf.Clamp(i, 0, pent.Length-1);

        int k = i + (int)SlopeToPitch(VelocitySlope);
        k = Mathf.Clamp(k, 0, pent.Length-1);
        return pent[k];
    }

    // Prewritten looping melody (keith jarrett koeln concert haha) sequenced with y position
    int[] melody = {-8, -3, -5, -3, -1, 0, -1, -3, -5, -8, -3, -8, -12, -8 -10, -10, -10};
    float melodyStepHeight = 0.5f;
    int MakePitch2() {
        i = (int)(Height/melodyStepHeight);
        i = i%melody.Length;

        return melody[i] + 12;
    }

    int MakePitch() {
        return MakePitch1();
    }

    void PlayStep() {
        UpdatePositionHash();

        UpdateFmodParams();
        _fmodParams[k_ParamPitch] = MakePitch();
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

    int crouch_i = 0;
    int[] crouch_seq = {0, -7};
    void PlayCrouch() {
        UpdateFmodParams();
        _fmodParams[k_ParamPitch] = crouch_seq[crouch_i++%2];
        // _fmodParams[k_ParamGain] = 5f;
        FMODPlayer.PlayEvent(new FMODEvent(m_CrouchEmitter, _fmodParams));
    }

    void StopCrouch() {
        m_CrouchEmitter.Stop();
    }

    void UpdateFmodParams() {
        // _fmodParams[k_ParamSlope]           = Slope;
        _fmodParams[k_ParamGain] = 0f;
        _fmodParams[k_ParamSpeedSquared]      = SpeedSquared;
        _fmodParams[k_ParamDeltaSpeedSquared] = DeltaSpeedSquared;
        _fmodParams[k_ParamIsOnGround]        = IsOnGround      ? 1f : 0f;
        _fmodParams[k_ParamIsOnWall]          = IsOnWall        ? 1f : 0f;
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
        int i = (int)(Mathf.InverseLerp(-1.05f, 1.05f, slope) * pitches.Length);
        return (float)pitches[i];
    }

    // -- queries --

    [ShowNativeProperty]
    float Height => State.Next.Position.y - k_StarterIslandBaseY; // ~0f at base to 

    [ShowNativeProperty]
    float NextJumpPower => State.NextJumpPower;
    
    [ShowNativeProperty]
    bool IsStepping => State.Next.Events.Contains(CharacterEvent.Step);

    [ShowNativeProperty]
    bool IsLanding => State.Next.Events.Contains(CharacterEvent.Land);

    [ShowNativeProperty]
    bool RecentlyLanded => Time.time - mostRecentLandedTime < k_RecentlyLandedLookbackSecs;

    [ShowNativeProperty]
    bool IsJumping => State.Next.Events.Contains(CharacterEvent.Jump);

    [ShowNativeProperty]
    // as in, is going into a crouch this frame
    bool IsCrouching => !State.Curr.IsCrouching && State.Next.IsCrouching;
    
    [ShowNativeProperty]
    // as in, is going into a crouch this frame
    bool IsStoppingCrouch => State.Curr.IsCrouching && !State.Next.IsCrouching;

    [ShowNativeProperty]
    float VelocitySlope => State.Next.Velocity.normalized.y; // -1 to 1

    [ShowNativeProperty]
    float SurfaceSlope => State.Next.MainSurface.Angle/180f; // 0 to 1?

    [ShowNativeProperty]
    float SpeedSquared => State.Next.Velocity.sqrMagnitude;

    [ShowNativeProperty]
    float DeltaSpeedSquared => (State.Next.Velocity - State.Curr.Velocity).sqrMagnitude;
    
    [ShowNativeProperty]
    float SpeedSquaredDelta => State.Next.Velocity.sqrMagnitude - State.Curr.Velocity.sqrMagnitude;

    [ShowNativeProperty]
    bool IsOnGround => State.Next.IsOnGround;

    [ShowNativeProperty]
    bool IsOnWall => State.Next.MainSurface.Angle > 30f;

    [ShowNativeProperty]
    bool IsHittingGround => !State.Curr.IsOnGround && State.Next.IsOnGround;

    [ShowNativeProperty]
    bool IsLeavingGround => State.Curr.IsOnGround && !State.Next.IsOnGround;
}

// Force redraw of exposed native properties in inspector every frame
#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(SimpleCharacterMusic))]
sealed class Editor: NaughtyInspector {
    public override bool RequiresConstantRepaint() => true;
}
#endif

}