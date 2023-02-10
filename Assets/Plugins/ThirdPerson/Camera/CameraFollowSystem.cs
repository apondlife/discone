using UnityEngine;
using UnityEngine.InputSystem;

namespace ThirdPerson {

sealed class CameraFollowSystem: System {
    // -- deps --
    /// the free look camera input
    InputAction m_Input;

    /// .
    CameraTuning m_Tuning;

    /// .
    CharacterState m_CharacterState;

    /// an offset at relative to the character pos
    Vector3 m_Offset;

    // -- props --
    /// the state-machine's state
    SystemState m_State;

    /// the current position of the camera
    public Vector3 m_CurrPosition { get; set; }

    // TODO: replace these three with a Spherical
    // the current yaw (rotation around the y-axis); relative to the zero dir
    float m_Yaw = 0.0f;

    // the current pitch (rotation around the x-axis)
    float m_Pitch = 0.0f;

    // the current radius
    float m_Radius = 0.0f;

     /// the current yaw speed
    float m_YawSpeed = 0.0f;

    /// the current pitch speed
    float m_PitchSpeed = 0.0f;

    /// the yaw world-direction at init
    Vector3 m_ZeroYawDir;

    // -- lifetime --
    public CameraFollowSystem(
        InputAction input,
        CameraTuning tuning,
        CharacterState characterState,
        Vector3 offset
    ) {
        // set deps
        m_Input = input;
        m_Tuning = tuning;
        m_CharacterState = characterState;
        m_Offset = offset;

        // set props
        m_ZeroYawDir = Vector3.ProjectOnPlane(-TargetForward, Vector3.up).normalized;
    }

    // -- System --
    protected override Phase InitInitialPhase() {
        return Tracking;
    }

    protected override SystemState State {
        get => m_State;
        set => m_State = value;
    }

    // -- Tracking --
    Phase Tracking => new Phase(
        name: "Tracking",
        update: Tracking_Update
    );

    void Tracking_Update(float delta) {
        if (m_Input.WasPerformedThisFrame()) {
            ChangeToImmediate(FreeLook, delta);
            return;
        }

        // move camera
        Tracking_Orbit(delta);
        Dolly(delta);
    }

    // -- FreeLook --
    Phase FreeLook => new Phase(
        name: "FreeLook",
        update: FreeLook_Update
    );

    void FreeLook_Update(float delta) {
        // move camera
        FreeLook_Orbit(delta);
        Dolly(delta);

        // if the player stops moving the camera, check their intentions
        if (!m_Input.WasPerformedThisFrame()) {
            ChangeTo(FreeLook_Intent);
            return;
        }
    }

    // -- FreeLook_Intent --
    Phase FreeLook_Intent => new Phase(
        name: "FreeLook_Intent",
        update: FreeLook_Intent_Update
    );

    void FreeLook_Intent_Update(float delta) {
        if (m_Input.WasPerformedThisFrame()) {
            ChangeToImmediate(FreeLook, delta);
            return;
        }

        // move camera
        FreeLook_Orbit(delta);
        Dolly(delta);

        // if the character moves, then we assume they intend to set the camera
        // for athletics
        if (!m_CharacterState.IsIdle) {
            ChangeTo(FreeLook_MoveIntent);
            return;
        }

        // if player doesnt move the camera for long enough, we assume the
        // player wants to look at the sky
        if (PhaseElapsed > m_Tuning.FreeLook_Timeout) {
            ChangeTo(FreeLook_IdleIntent);
            return;
        }
    }

    // -- FreeLook_MoveIntent --
    Phase FreeLook_MoveIntent => new Phase(
        name: "FreeLook_MoveIntent",
        update: FreeLook_MoveIntent_Update
    );

    void FreeLook_MoveIntent_Update(float delta) {
        if (m_Input.WasPerformedThisFrame()) {
            ChangeToImmediate(FreeLook, delta);
            return;
        }

        // move camera
        FreeLook_Orbit(delta);
        Dolly(delta);

        // if the player sits around for a while after moving, we assume they've
        // finished moving and reset the camera
        if (m_CharacterState.IdleTime > m_Tuning.FreeLook_MoveIntentTimeout) {
            ChangeTo(Tracking);
            return;
        }
    }

    // -- FreeLook_IdleIntent --
    Phase FreeLook_IdleIntent => new Phase(
        name: "FreeLook_IdleIntent",
        update: FreeLook_Idle_Update
    );

    void FreeLook_Idle_Update(float delta) {
        if (m_Input.WasPerformedThisFrame()) {
            ChangeToImmediate(FreeLook, delta);
            return;
        }

        // move camera
        FreeLook_Orbit(delta);
        Dolly(delta);

        // if the player starts moving, we assume the camera for looking at the
        // sky is not so useful anymore
        if (!m_CharacterState.IsIdle) {
            ChangeTo(Tracking);
            return;
        }
    }

    // -- commands --
    void Tracking_Orbit(float delta) {
        // m_Yaw can be wrong in collisions, but why not use this?
        // var yawRot = Quaternion.AngleAxis(m_Yaw, Vector3.up);
        // var currFwd = yawRot * m_ZeroYawDir;

        var currDir = m_CurrPosition - TargetPosition;
        var currFwd = Vector3.ProjectOnPlane(currDir, Vector3.up);

        // get current yaw
        var currYaw = Vector3.SignedAngle(
            m_ZeroYawDir,
            currFwd,
            Vector3.up
        );

        // get desired yaw behind model
        var destFwd = -Vector3.ProjectOnPlane(TargetForward, Vector3.up);
        var destYaw = Vector3.SignedAngle(
            m_ZeroYawDir,
            destFwd,
            Vector3.up
        );

        // sample yaw speed along recenter / active curve & accelerate towards it
        var yawDisp = Mathf.Abs(destYaw - currYaw);
        var destYawSpeed = m_CharacterState.IdleTime > m_Tuning.Recenter_IdleTime
            ? Mathf.Lerp(0, m_Tuning.Recenter_YawSpeed, m_Tuning.Recenter_YawCurve.Evaluate(yawDisp / 180.0f))
            : Mathf.Lerp(0, m_Tuning.Tracking_YawSpeed, m_Tuning.Tracking_YawCurve.Evaluate(yawDisp / 180.0f));

        // integrate yaw acceleration
        m_YawSpeed = Mathf.MoveTowards(
            m_YawSpeed,
            destYawSpeed,
            m_Tuning.YawAcceleration * delta
        );

        // integrate yaw speed
        var nextYaw = Mathf.MoveTowardsAngle(
            currYaw,
            destYaw,
            m_YawSpeed * delta
        );

        // rotate pitch on the plane containing the target's position and up
        // TODO: lerp this based on m_State.LookAtTarget_PercentExtended
        var destPitch = Mathf.LerpAngle(
            m_Tuning.Tracking_MinPitch,
            m_Tuning.Tracking_MaxPitch,
            0.0f
        );

        m_PitchSpeed = Mathf.MoveTowards(
            m_PitchSpeed,
            m_Tuning.Tracking_PitchSpeed,
            m_Tuning.Tracking_PitchAcceleration * delta
        );

        var nextPitch = Mathf.MoveTowardsAngle(
            m_Pitch,
            destPitch,
            m_PitchSpeed * delta
        );

        // update state
        m_Yaw = nextYaw;
        m_Pitch = nextPitch;
    }

    void FreeLook_Orbit(float delta) {
        // get camera input
        var input = m_Input.ReadValue<Vector2>();
        input.x = m_Tuning.IsInvertedX ? -input.x : input.x;
        input.y = m_Tuning.IsInvertedY ? -input.y : input.y;

        var currDir = m_CurrPosition - TargetPosition;
        var currFwd = Vector3.ProjectOnPlane(currDir, Vector3.up);

        // get current yaw
        var currYaw = Vector3.SignedAngle(
            m_ZeroYawDir,
            currFwd,
            Vector3.up
        );

        // integrate yaw acceleration
        m_YawSpeed = Mathf.MoveTowards(
            m_YawSpeed,
            m_Tuning.FreeLook_YawSpeed * -input.x,
            m_Tuning.FreeLook_YawAcceleration * delta
        );

        // integrate updated yaw
        var nextYaw = currYaw + m_YawSpeed * delta;

        // get current pitch
        var currPitch = Mathf.Rad2Deg * Mathf.Atan2(currDir.y, currFwd.magnitude);

        // integrate pitch acceleration
        m_PitchSpeed = Mathf.MoveTowards(
            m_PitchSpeed,
            m_Tuning.FreeLook_PitchSpeed * input.y,
            m_Tuning.FreeLook_PitchAcceleration * delta
        );

        // integrate updated pitch
        var nextPitch = Mathf.MoveTowardsAngle(
            currPitch,
            currPitch + m_PitchSpeed * delta,
            Mathf.Abs(m_PitchSpeed * delta)
        );

        nextPitch = Mathf.Clamp(
            nextPitch,
            m_Tuning.FreeLook_MinPitch,
            m_Tuning.FreeLook_MaxPitch
        );

        // update state
        // TODO: integrate radius
        m_Yaw = nextYaw;
        m_Pitch = nextPitch;
        m_Radius = currDir.magnitude;
    }

    void Dolly(float delta) {
        // dolly back; scale dolly radius based on character speed
        var radiusScale = Mathf.Lerp(
            1.0f,
            m_Tuning.MaxRadius / m_Tuning.MinRadius,
            m_Tuning.DollySpeedCurve.Evaluate(Mathf.InverseLerp(
                m_Tuning.DollyTargetMinSpeed,
                m_Tuning.DollyTargetMaxSpeed,
                m_CharacterState.Next.PlanarVelocity.magnitude
            ))
        );

        // integrate dolly speed
        var nextRadius =  Mathf.MoveTowards(
            m_Radius,
            m_Tuning.MinRadius * radiusScale,
            m_Tuning.DollySpeed * delta
        );

        // update radius
        m_Radius = nextRadius;
    }

    // -- queries --
    public Vector3 DestPosition() {
        // calc dest forward from yaw
        var yawRot = Quaternion.AngleAxis(m_Yaw, Vector3.up);
        var yawFwd = yawRot * m_ZeroYawDir;

        // rotate pitch on the plane containing the target's forward and up
        var pitchRot = Quaternion.AngleAxis(
            m_Pitch,
            Vector3.Cross(yawFwd, Vector3.up).normalized
        );

        return TargetPosition + pitchRot * yawFwd * m_Radius;
    }

    Vector3 TargetForward {
        get => m_CharacterState.Forward;
    }

    Vector3 TargetPosition {
        get => m_CharacterState.Position + m_Offset;
    }
}

}