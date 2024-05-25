using System;
using Soil;
using UnityEngine;

namespace ThirdPerson {

[Serializable]
sealed class CameraFollowSystem: SimpleSystem<CameraContainer> {
    // -- System --
    protected override Phase<CameraContainer> InitInitialPhase() {
        return Idle;
    }

    public override void Init(CameraContainer c) {
        base.Init(c);

        // set initial state
        c.State.Next.Spherical.Radius = c.Tuning.MinRadius;
        c.State.Next.Spherical.Azimuth = 0f;
        c.State.Next.Spherical.Zenith = c.Tuning.Tracking_MinPitch;
    }

    // -- Idle --
    // player not moving and not controlling the camera
    static readonly Phase<CameraContainer> Idle = new("Idle",
        update: (delta, s, c) => {
            if (c.Input.IsPressed()) {
                s.ChangeToImmediate(FreeLook, delta);
                return;
            }

            if (!c.CharacterInput.IsMoveIdle(c.Tuning.Tracking_IdleFrames)) {
                s.ChangeToImmediate(Tracking, delta);
                return;
            }

            c.State.Next.Spherical = c.State.IntoCurrSpherical();
        }
    );

    // -- Tracking --
    // camera following the player on its own
    static readonly Phase<CameraContainer> Tracking = new("Tracking",
        update: (delta, s, c) => {
            if (c.Input.IsPressed()) {
                s.ChangeToImmediate(FreeLook, delta);
                return;
            }

            // move camera
            Tracking_Orbit(delta, false, c);
            Dolly(delta, c);

            // stop tracking wnen move input becomes idle
            if (c.CharacterInput.IsMoveIdle(c.Tuning.Tracking_IdleFrames)) {
                s.ChangeTo(Idle);
                return;
            }
        }
    );

    // -- FreeLook --
    // player controlling the camera
    static readonly Phase<CameraContainer> FreeLook = new("FreeLook",
        enter: (_, c) => {
            c.State.Next.IsFreeLook = true;
        },
        update: (delta, s, c) => {
            // move camera
            FreeLook_Orbit(delta, c);
            Dolly(delta, c);

            // if the player stops moving the camera, check their intentions
            if (!c.Input.IsPressed()) {
                s.ChangeTo(FreeLook_Intent);
                return;
            }
        }
    );

    // -- FreeLook_Intent --
    static readonly Phase<CameraContainer> FreeLook_Intent = new("FreeLook_Intent",
        update: (delta, s, c) => {
            if (c.Input.IsPressed()) {
                s.ChangeToImmediate(FreeLook, delta);
                return;
            }

            // move camera
            FreeLook_Orbit(delta, c);
            Dolly(delta, c);

            // if the character moves, then we assume they intend to set the camera
            // for athletics
            if (!c.State.Character.IsIdle) {
                s.ChangeTo(FreeLook_MoveIntent);
                return;
            }

            // if player doesnt move the camera for long enough, we assume the
            // player wants to look at the sky
            if (s.PhaseElapsed > c.Tuning.FreeLook_Timeout) {
                s.ChangeTo(FreeLook_IdleIntent);
                return;
            }
        }
    );

    // -- FreeLook_MoveIntent --
    static readonly Phase<CameraContainer> FreeLook_MoveIntent = new("FreeLook_MoveIntent",
        update: (delta, s, c) => {
            if (c.Input.IsPressed()) {
                s.ChangeToImmediate(FreeLook, delta);
                return;
            }

            // move camera
            FreeLook_Orbit(delta, c);
            Dolly(delta, c);

            // if the player sits around for a while after moving, we assume they've
            // finished moving and reset the camera
            if (c.State.Character.IdleTime > c.Tuning.FreeLook_MoveIntentTimeout) {
                s.ChangeTo(Idle);
                return;
            }
        },
        exit: (s, c) => {
            c.State.Next.IsFreeLook = false;
        }
    );

    // -- FreeLook_IdleIntent --
    static readonly Phase<CameraContainer> FreeLook_IdleIntent = new("FreeLook_IdleIntent",
        update: (delta, s, c) => {
            if (c.Input.WasPerformedThisFrame()) {
                s.ChangeToImmediate(FreeLook, delta);
                return;
            }

            // move camera
            FreeLook_Orbit(delta, c);
            Dolly(delta, c);

            // if the player starts moving, we assume the camera for looking at the
            // sky is not so useful anymore
            if (!c.State.Character.IsIdle) {
                s.ChangeTo(Tracking);
                return;
            }
        },
        exit: (_, c) => {
            c.State.Next.IsFreeLook = false;
        }
    );

    // -- commands --
    /// resolve tracking camera orbit
    static void Tracking_Orbit(float delta, bool isRecentering, CameraContainer c) {
        // TODO: yaw speed could be wrong at this point (yawSpeed != deltaYaw/deltaTime).
        // we should resample yaw speed from the current state.

        // get current yaw
        var currYaw = c.State.Spherical.Azimuth;

        // get desired yaw behind model
        var destFwd = -Vector3.ProjectOnPlane(c.State.FollowForward, Vector3.up);
        var destYaw = Vector3.SignedAngle(
            c.State.FollowYawZeroDir,
            destFwd,
            Vector3.up
        );

        // sample yaw speed along recenter / active curve & accelerate towards it
        var deltaYaw = Mathf.DeltaAngle(currYaw, destYaw);
        var deltaYawMag = Mathf.Abs(deltaYaw);
        var deltaYawDir = Mathf.Sign(deltaYaw);

        // TODO: make these range curves
        var destYawSpeed = isRecentering
            ? Mathf.Lerp(0, c.Tuning.Recenter_YawSpeed, c.Tuning.Recenter_YawCurve.Evaluate(deltaYawMag / 180.0f))
            : Mathf.Lerp(0, c.Tuning.Tracking_YawSpeed, c.Tuning.Tracking_YawCurve.Evaluate(deltaYawMag / 180.0f));

        // TODO: make sure recenter actually goes all the way to the back of the character, instead of accelerating forever
        var yawAcceleration = isRecentering
            ? c.Tuning.Recenter_YawAcceleration
            : c.Tuning.YawAcceleration;

        // integrate yaw acceleration
        var nextYawSpeed = Mathf.MoveTowards(
            c.State.Curr.Velocity.Azimuth,
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
        // TODO: lerp this based on c.State.LookAtTarget_PercentExtended
        var destPitch = Mathf.LerpAngle(
            c.Tuning.Tracking_MinPitch,
            c.Tuning.Tracking_MaxPitch,
            0.0f
        );

        var nextPitchSpeed = Mathf.MoveTowards(
            c.State.Curr.Velocity.Zenith,
            c.Tuning.Tracking_PitchSpeed,
            c.Tuning.Tracking_PitchAcceleration * delta
        );

        var nextPitch = Mathf.MoveTowardsAngle(
            c.State.Curr.Spherical.Zenith,
            destPitch,
            nextPitchSpeed * delta
        );

        // update state
        var next = c.State.Next;
        next.Spherical.Azimuth = nextYaw;
        next.Spherical.Zenith = nextPitch;

        next.Velocity.Azimuth = nextYawSpeed;
        next.Velocity.Zenith = nextPitchSpeed;
    }

    /// resolve free look camera orbit
    static void FreeLook_Orbit(float delta, CameraContainer c) {
        // get camera input
        var input = c.Input.ReadValue<Vector2>();
        input.x = c.Tuning.IsInvertedX ? -input.x : input.x;
        input.y = c.Tuning.IsInvertedY ? -input.y : input.y;

        // integrate yaw acceleration
        var nextYawSpeed = Mathf.MoveTowards(
            c.State.Curr.Velocity.Azimuth,
            c.Tuning.FreeLook_YawSpeed * -input.x,
            c.Tuning.FreeLook_YawAcceleration * delta
        );

        // integrate updated yaw
        var currYaw = c.State.Curr.Spherical.Azimuth;
        var nextYaw = Mathf.MoveTowardsAngle(
            currYaw,
            currYaw + nextYawSpeed * delta,
            float.MaxValue
        );

        // integrate pitch acceleration
        var nextPitchSpeed = Mathf.MoveTowards(
            c.State.Curr.Velocity.Zenith,
            c.Tuning.FreeLook_PitchSpeed * input.y,
            c.Tuning.FreeLook_PitchAcceleration * delta
        );

        // integrate updated pitch
        var currPitch = c.State.Curr.Spherical.Zenith;
        var nextPitch = Mathf.MoveTowardsAngle(
            currPitch,
            currPitch + nextPitchSpeed * delta,
            float.MaxValue
        );

        nextPitch = Mathf.Clamp(
            nextPitch,
            c.Tuning.FreeLook_MinPitch,
            c.Tuning.FreeLook_MaxPitch
        );

        // update state
        var next = c.State.Next;
        next.Spherical.Azimuth = nextYaw;
        next.Spherical.Zenith = nextPitch;

        next.Velocity.Azimuth = nextYawSpeed;
        next.Velocity.Zenith = nextPitchSpeed;
    }

    /// dolly in or out
    static void Dolly(float delta, CameraContainer c) {
        // only dolly if not colliding
        if (c.State.Curr.IsColliding) {
            return;
        }

        // dolly back; scale dolly radius based on character speed
        var currRadius = c.State.Curr.Spherical.Radius;
        var radiusScale = Mathf.Lerp(
            1.0f,
            c.Tuning.MaxRadius / c.Tuning.MinRadius,
            c.Tuning.DollySpeedCurve.Evaluate(Mathf.InverseLerp(
                c.Tuning.DollyTargetMinSpeed,
                c.Tuning.DollyTargetMaxSpeed,
                c.State.Character.Next.Velocity.magnitude
            ))
        );

        // integrate dolly speed
        var destRadius = c.Tuning.MinRadius * radiusScale;
        var nextRadius =  Mathf.MoveTowards(
            currRadius,
            destRadius,
            c.Tuning.DollySpeed * delta
        );

        // update radius
        var next = c.State.Next;
        next.Spherical.Radius = nextRadius;
    }
}

}