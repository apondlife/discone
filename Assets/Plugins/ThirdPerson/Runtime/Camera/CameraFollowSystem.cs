using System;
using UnityEngine;

namespace ThirdPerson {

[Serializable]
sealed class CameraFollowSystem: CameraSystem {
    // -- props --
    // angle (in degrees) that camera is away from its target yaw
    float m_DeltaYawMag;

    // -- System --
    protected override Phase InitInitialPhase() {
        return Idle;
    }

    public override void Init() {
        base.Init();

        // set initial state
        m_State.Next.Spherical.Radius = m_Tuning.MinRadius;
        m_State.Next.Spherical.Azimuth = 0f;
        m_State.Next.Spherical.Zenith = m_Tuning.Tracking_MinPitch;
    }

    // -- Idle --
    // player not moving and not controlling the camera
    Phase Idle => new Phase(
        name: "Idle",
        update: Idle_Update
    );

    void Idle_Update(float delta) {
        if (m_Input.IsPressed()) {
            ChangeToImmediate(FreeLook, delta);
            return;
        }

        if (!m_CharacterInput.IsMoveIdle(m_Tuning.Tracking_IdleFrames)) {
            ChangeToImmediate(Tracking, delta);
            return;
        }

        m_State.Next.Spherical = m_State.IntoCurrSpherical();
    }

    // -- Tracking --
    // camera following the player on its own
    Phase Tracking => new Phase(
        name: "Tracking",
        update: Tracking_Update
    );

    void Tracking_Update(float delta) {
        if (m_Input.IsPressed()) {
            ChangeToImmediate(FreeLook, delta);
            return;
        }

        // move camera
        Tracking_Orbit(delta);
        Dolly(delta);

        // stop tracking wnen move input becomes idle
        if (m_CharacterInput.IsMoveIdle(m_Tuning.Tracking_IdleFrames)) {
            ChangeTo(Idle);
            return;
        }
    }

    // -- FreeLook --
    // player controlling the camera
    Phase FreeLook => new Phase(
        name: "FreeLook",
        enter: FreeLook_Enter,
        update: FreeLook_Update
    );

    void FreeLook_Enter() {
        m_State.Next.IsFreeLook = true;
    }

    void FreeLook_Update(float delta) {
        // move camera
        FreeLook_Orbit(delta);
        Dolly(delta);

        // if the player stops moving the camera, check their intentions
        if (!m_Input.IsPressed()) {
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
        if (m_Input.IsPressed()) {
            ChangeToImmediate(FreeLook, delta);
            return;
        }

        // move camera
        FreeLook_Orbit(delta);
        Dolly(delta);

        // if the character moves, then we assume they intend to set the camera
        // for athletics
        if (!m_State.Character.IsIdle) {
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
        update: FreeLook_MoveIntent_Update,
        exit: FreeLook_MoveIntent_Exit
    );

    void FreeLook_MoveIntent_Update(float delta) {
        if (m_Input.IsPressed()) {
            ChangeToImmediate(FreeLook, delta);
            return;
        }

        // move camera
        FreeLook_Orbit(delta);
        Dolly(delta);

        // if the player sits around for a while after moving, we assume they've
        // finished moving and reset the camera
        if (m_State.Character.IdleTime > m_Tuning.FreeLook_MoveIntentTimeout) {
            ChangeTo(Idle);
            return;
        }
    }

    void FreeLook_MoveIntent_Exit() {
        m_State.Next.IsFreeLook = false;
    }

    // -- FreeLook_IdleIntent --
    Phase FreeLook_IdleIntent => new Phase(
        name: "FreeLook_IdleIntent",
        update: FreeLook_IdleIntent_Update,
        exit: FreeLook_IdleIntent_Exit
    );

    void FreeLook_IdleIntent_Update(float delta) {
        if (m_Input.WasPerformedThisFrame()) {
            ChangeToImmediate(FreeLook, delta);
            return;
        }

        // move camera
        FreeLook_Orbit(delta);
        Dolly(delta);

        // if the player starts moving, we assume the camera for looking at the
        // sky is not so useful anymore
        if (!m_State.Character.IsIdle) {
            ChangeTo(Tracking);
            return;
        }
    }

    void FreeLook_IdleIntent_Exit() {
        m_State.Next.IsFreeLook = false;
    }

    // -- commands --
    /// resolve tracking camera orbit
    void Tracking_Orbit(float delta, bool isRecentering = false) {
        // TODO: yaw speed could be wrong at this point (yawSpeed !=
        // deltaYaw/deltaTime). we should resample yaw speed from the current
        // state.

        // get current yaw
        var currYaw = m_State.Spherical.Azimuth;

        // get desired yaw behind model
        var destFwd = -Vector3.ProjectOnPlane(m_State.FollowForward, Vector3.up);
        var destYaw = Vector3.SignedAngle(
            m_State.FollowYawZeroDir,
            destFwd,
            Vector3.up
        );

        // sample yaw speed along recenter / active curve & accelerate towards it
        var deltaYaw = Mathf.DeltaAngle(currYaw, destYaw);
        var deltaYawMag = Mathf.Abs(deltaYaw);
        var deltaYawDir = Mathf.Sign(deltaYaw);

        // TODO: make these range curves
        var destYawSpeed = isRecentering
            ? Mathf.Lerp(0, m_Tuning.Recenter_YawSpeed, m_Tuning.Recenter_YawCurve.Evaluate(deltaYawMag / 180.0f))
            : Mathf.Lerp(0, m_Tuning.Tracking_YawSpeed, m_Tuning.Tracking_YawCurve.Evaluate(deltaYawMag / 180.0f));

        // TODO: make sure recenter actually goes all the way to the back of the character, instead of accelerating forever
        var yawAcceleration = isRecentering
            ? m_Tuning.Recenter_YawAcceleration
            : m_Tuning.YawAcceleration;

        // integrate yaw acceleration
        var nextYawSpeed = Mathf.MoveTowards(
            m_State.Curr.Velocity.Azimuth,
            deltaYawDir * destYawSpeed,
            yawAcceleration * delta
        );

        // integrate yaw speed
        var nextYaw = Mathf.MoveTowardsAngle(
            currYaw,
            destYaw,
            Mathf.Abs(nextYawSpeed * delta)
        );

        // rotate pitch on the plane containing the target's position and up
        // TODO: lerp this based on m_State.LookAtTarget_PercentExtended
        var destPitch = Mathf.LerpAngle(
            m_Tuning.Tracking_MinPitch,
            m_Tuning.Tracking_MaxPitch,
            0.0f
        );

        var nextPitchSpeed = Mathf.MoveTowards(
            m_State.Curr.Velocity.Zenith,
            m_Tuning.Tracking_PitchSpeed,
            m_Tuning.Tracking_PitchAcceleration * delta
        );

        var nextPitch = Mathf.MoveTowardsAngle(
            m_State.Curr.Spherical.Zenith,
            destPitch,
            nextPitchSpeed * delta
        );

        // update state
        var next = m_State.Next;
        next.Spherical.Azimuth = nextYaw;
        next.Spherical.Zenith = nextPitch;

        next.DestSpherical.Azimuth = destYaw;
        next.DestSpherical.Zenith = destPitch;

        next.Velocity.Azimuth = nextYawSpeed;
        next.Velocity.Zenith = nextPitchSpeed;

        m_DeltaYawMag = deltaYawMag;
    }

    /// resolve free look camera orbit
    void FreeLook_Orbit(float delta) {
        // get camera input
        var input = m_Input.ReadValue<Vector2>();
        input.x = m_Tuning.IsInvertedX ? -input.x : input.x;
        input.y = m_Tuning.IsInvertedY ? -input.y : input.y;

        // integrate yaw acceleration
        var nextYawSpeed = Mathf.MoveTowards(
            m_State.Curr.Velocity.Azimuth,
            m_Tuning.FreeLook_YawSpeed * -input.x,
            m_Tuning.FreeLook_YawAcceleration * delta
        );

        // integrate updated yaw
        var currYaw = m_State.Curr.Spherical.Azimuth;
        var nextYaw = Mathf.MoveTowardsAngle(
            currYaw,
            currYaw + nextYawSpeed * delta,
            float.MaxValue
        );

        // integrate pitch acceleration
        var nextPitchSpeed = Mathf.MoveTowards(
            m_State.Curr.Velocity.Zenith,
            m_Tuning.FreeLook_PitchSpeed * input.y,
            m_Tuning.FreeLook_PitchAcceleration * delta
        );

        // integrate updated pitch
        var currPitch = m_State.Curr.Spherical.Zenith;
        var nextPitch = Mathf.MoveTowardsAngle(
            currPitch,
            currPitch + nextPitchSpeed * delta,
            float.MaxValue
        );

        nextPitch = Mathf.Clamp(
            nextPitch,
            m_Tuning.FreeLook_MinPitch,
            m_Tuning.FreeLook_MaxPitch
        );

        // update state
        var next = m_State.Next;
        next.Spherical.Azimuth = nextYaw;
        next.Spherical.Zenith = nextPitch;

        next.DestSpherical.Azimuth = nextYaw;
        next.DestSpherical.Zenith = nextPitch;

        next.Velocity.Azimuth = nextYawSpeed;
        next.Velocity.Zenith = nextPitchSpeed;
    }

    /// dolly in or out
    void Dolly(float delta) {
        // only dolly if not colliding
        if (m_State.Curr.IsColliding) {
            return;
        }

        // dolly back; scale dolly radius based on character speed
        var currRadius = m_State.Curr.Spherical.Radius;
        var radiusScale = Mathf.Lerp(
            1.0f,
            m_Tuning.MaxRadius / m_Tuning.MinRadius,
            m_Tuning.DollySpeedCurve.Evaluate(Mathf.InverseLerp(
                m_Tuning.DollyTargetMinSpeed,
                m_Tuning.DollyTargetMaxSpeed,
                m_State.Character.Next.Velocity.magnitude
            ))
        );

        // integrate dolly speed
        var destRadius = m_Tuning.MinRadius * radiusScale;
        var nextRadius =  Mathf.MoveTowards(
            currRadius,
            destRadius,
            m_Tuning.DollySpeed * delta
        );

        // update radius
        var next = m_State.Next;
        next.Spherical.Radius = nextRadius;
        next.DestSpherical.Radius = destRadius;
    }
}

}