using System;
using Soil;
using UnityEngine;

namespace ThirdPerson {

[Serializable]
sealed class CameraCollisionSystem: SimpleSystem<CameraContainer> {
    // -- System --
    protected override Phase<CameraContainer> InitInitialPhase() {
        return Tracking;
    }

    public override void Init(CameraContainer c) {
        base.Init(c);

        // set initial state
        c.State.Next.Pos = c.State.IntoIdealPosition();
    }

    // -- Tracking --
    static readonly Phase<CameraContainer> Tracking = new("Tracking",
        update: (delta, s, c) => {
            if (c.State.IsFreeLook) {
                s.ChangeToImmediate(FreeLook, delta);
                return;
            }

            var ideal = c.State.IntoIdealPosition();
            var corrected = GetTrackingPos(ideal, c);
            c.State.Next.Pos = ideal;

            if (ideal != corrected) {
                s.ChangeTo(Tracking_Correcting);
                return;
            }
        }
    );

    // -- Tracking_Correcting --
    static readonly Phase<CameraContainer> Tracking_Correcting = new("Tracking_Correcting",
        update: (delta, s, c) => {
            if (c.State.IsFreeLook) {
                s.ChangeToImmediate(FreeLook, delta);
                return;
            }

            var idealPos = c.State.IntoIdealPosition();
            var correctPos = GetTrackingPos(idealPos, c);

            var nextPos = Vector3.MoveTowards(
                c.State.Curr.Pos,
                correctPos,
                c.Tuning.Collision_Tracking_CorrectionSpeed * delta
            );

            c.State.Next.Pos = nextPos;

            if (nextPos == idealPos) {
                s.ChangeTo(Tracking);
                return;
            }
        }
    );

    // -- FreeLook --
    static readonly Phase<CameraContainer> FreeLook = new("FreeLook",
        update: (delta, s, c) => {
            if (!c.State.IsFreeLook) {
                s.ChangeToImmediate(Tracking, delta);
                return;
            }

            var ideal = c.State.IntoIdealPosition();
            var corrected = GetFreeLookPos(ideal, c);

            c.State.Next.Pos = ideal;

            if (ideal != corrected) {
                s.ChangeTo(FreeLook_Colliding);
                return;
            }
        }
    );

    // -- FreeLook_Colliding --
    static readonly Phase<CameraContainer> FreeLook_Colliding = new("FreeLook_Colliding",
        update: (delta, s, c) => {
            if (!c.State.IsFreeLook) {
                s.ChangeToImmediate(Tracking, delta);
                return;
            }

            var ideal = c.State.IntoIdealPosition();
            var corrected = GetFreeLookPos(ideal, c);

            // c.State.Next.Pos = c.State.Curr.Pos + (inputImpulse + correctionImpulse);
            if (ideal == corrected) {
                s.ChangeToImmediate(FreeLook, delta);
                return;
            }

            // scale tolerance with hit normal
            var normalDotUp = Vector3.Dot(c.State.HitNormal, Vector3.up);
            var tolerance = c.Tuning.Collision_ClipToleranceByNormal.Evaluate(normalDotUp);
            var mag = Vector3.Magnitude(ideal - corrected);
            if (mag > tolerance) {
                s.ChangeToImmediate(FreeLook_Clipping, delta);
                return;
            }

            // interpolate towards corrected position while colliding
            c.State.Next.Pos = Vector3.MoveTowards(
                c.State.Next.Pos,
                corrected,
                c.Tuning.Collision_FreeLook_CorrectionSpeed * delta
            );
        }
    );

    // -- FreeLook_Clipping --
    static readonly Phase<CameraContainer> FreeLook_Clipping = new("FreeLook_Clipping",
        enter: (s, c) => {
            c.State.Next.Velocity *= 1f - c.Tuning.Collision_ClipDamping.Evaluate(s.PhaseStart, releaseStartTime: s.PhaseStart);
        },
        update: (delta, s, c) => {
            if (!c.State.IsFreeLook) {
                s.ChangeToImmediate(Tracking, delta);
                return;
            }

            var ideal = c.State.IntoIdealPosition();
            var corrected = GetFreeLookPos(ideal, c);

            c.State.Next.Pos = ideal;
            c.State.Next.Velocity *= 1.0f - c.Tuning.Collision_ClipDamping.Evaluate(s.PhaseStart, releaseStartTime: s.PhaseStart);

            if (ideal == corrected) {
                s.ChangeTo(FreeLook_ClippingCooldown);
                return;
            }
        }
    );

    // -- FreeLook_ClippingCooldown --
    static readonly Phase<CameraContainer> FreeLook_ClippingCooldown = new("FreeLook_ClippingCooldown",
        update: (delta, s, c) => {
            if (!c.State.IsFreeLook) {
                s.ChangeToImmediate(Tracking, delta);
                return;
            }

            var ideal = c.State.IntoIdealPosition();
            var corrected = GetFreeLookPos(ideal, c);

            c.State.Next.Pos = ideal;

            if (ideal != corrected) {
                s.ChangeTo(FreeLook_Clipping);
                return;
            }

            if (s.PhaseElapsed >= c.Tuning.Collision_ClipCooldown) {
                s.ChangeTo(FreeLook);
                return;
            }
        }
    );

    // -- queries --
    static Vector3 GetFreeLookPos(Vector3 candidate, CameraContainer c) {
        // the final position
        var destPos = candidate;

        // the character's position
        var origin = c.State.FollowPosition;

        // step 1: cast from the character to the ideal position to see if any
        // surface is blocking visibility; use a sphere cast so we don't get
        // closer than the contact offset
        var vizCastStart = candidate - origin;
        var vizLen = vizCastStart.magnitude;
        var vizDir = vizCastStart.normalized;

        var didHit = Physics.SphereCast(
            origin,
            c.Tuning.Collision_ContactOffset,
            vizDir,
            out var hit,
            vizLen,
            c.Tuning.Collision_Mask,
            QueryTriggerInteraction.Ignore
        );

        // TODO: don't set state in here
        c.State.Next.IsColliding = didHit;
        c.State.HitPos = hit.point;
        c.State.HitNormal = hit.normal;

        // if the target is visible, we have our desired position
        if (!didHit) {
            return destPos;
        }

        // otherwise, we found the point on this surface (note: offset by c.o.
        // so that step 3b works)
        var vizNormal = hit.normal;
        var vizPos = OffsetHit(hit, c);

        destPos = vizPos;

        // step 2: project the candidate along the plane of the hit surface
        // using the remaining distance to candidate

        // scale the projection down if the pitch is < 0 so that we can pan
        // into the character
        // TODO: add a tuning to scale this differently
        var projK = 1.0f;
        var pitch = c.State.Next.Spherical.Zenith;
        if (pitch < 0.0f) {
            projK = 1.0f - pitch / c.Tuning.FreeLook_MinPitch;
        }

        var projLen = Vector3.Distance(candidate, vizPos);
        var projDir = Vector3.Cross(vizNormal, Vector3.Cross(vizDir, vizNormal)).normalized;
        var projPos = vizPos + projK * projLen * projDir;

        destPos = projPos;

        return destPos;
    }

    /// correct camera position in attempt to preserve line of sight
    /// see: https://miro.com/app/board/uXjVOWfpI6I=/?moveToWidget=3458764535240497690&cot=14
    static Vector3 GetTrackingPos(Vector3 candidate, CameraContainer c) {
        // the final position
        var destPos = candidate;

        // the character's position
        var origin = c.State.FollowPosition;

        // step 1: cast from the character to the ideal position to see if any
        // surface is blocking visibility; use a sphere cast so we don't get
        // closer than the contact offset
        var vizCastStart = candidate - origin;
        var vizLen = vizCastStart.magnitude;
        var vizDir = vizCastStart.normalized;

        var didHit = Physics.SphereCast(
            origin,
            c.Tuning.Collision_ContactOffset,
            vizDir,
            out var hit,
            vizLen,
            c.Tuning.Collision_Mask,
            QueryTriggerInteraction.Ignore
        );

        // TODO: don't set state in here
        c.State.Next.IsColliding = didHit;
        c.State.HitPos = hit.point;
        c.State.HitNormal = hit.normal;

        // if the target is visible, we have our desired position
        if (!didHit) {
            return destPos;
        }

        // otherwise, we found the point on this surface (note: offset by c.o.
        // so that step 3b works)
        var vizNormal = hit.normal;
        var vizPos = OffsetHit(hit, c);

        destPos = vizPos;

        // step 2: project the candidate along the plane of the hit surface
        // using the remaining distance to candidate

        // scale the projection down if the pitch is < 0 so that we can pan
        // into the character
        var projK = 1.0f;
        var pitch = c.State.Next.Spherical.Zenith;
        if (pitch < 0.0f) {
            projK = 1.0f - pitch / c.Tuning.FreeLook_MinPitch;
        }

        var projLen = Vector3.Distance(candidate, vizPos);
        var projDir = Vector3.Cross(vizNormal, Vector3.Cross(vizDir, vizNormal)).normalized;
        var projPos = vizPos + projK * projLen * projDir;

        destPos = projPos;

        // step 3: we may have projected ourselves into objects or into a place
        // that is occluded (e.g. by a doorframe), so try and escape these
        // problems

        // step 3.a: first, try to exit vertically. if a surface is blocking the
        // camera, cast up and down along the vertical axis containing the ideal
        // position in the direction of the surface.
        var exitVertDir = Mathf.Sign(vizNormal.y) * Vector3.up;
        var distance = Vector3.Distance(candidate, origin);

        var exitVertLen = 2.0f * distance; // the "diamater" of the curve TODO: is this right?

        didHit = Physiics.BounceCast(
            destPos,
            exitVertDir,
            out hit,
            exitVertLen,
            c.Tuning.Collision_Mask,
            QueryTriggerInteraction.Ignore
        );

        if (didHit) {
            destPos = OffsetHit(hit, c);
        }

        // step 3.b: if we didn't exit vertically, we're still in the viz plane.
        // try to exit along it to deal with occluders like doorways by casting
        // from the viz pos to dest pos.
        if (!didHit) {
            var exitPlaneSrc = vizPos;
            var exitPlaneDst = destPos;

            didHit = Physics.Linecast(
                exitPlaneSrc,
                exitPlaneDst,
                out hit,
                c.Tuning.Collision_Mask,
                QueryTriggerInteraction.Ignore
            );

            if (didHit) {
                destPos = OffsetHit(hit, c);
            }
        }

        // step 4: do a final vision cast from the player to make sure the destination
        // point is visible.
        var vizCastEndSrc = origin;
        var vizCastEndDst = destPos;

        didHit = Physics.Linecast(
            vizCastEndSrc,
            vizCastEndDst,
            out hit,
            c.Tuning.Collision_Mask,
            QueryTriggerInteraction.Ignore
        );

        if (didHit) {
            destPos = OffsetHit(hit, c);
        }

        return destPos;
    }

    /// the hit point adjusted by the contact offset
    static Vector3 OffsetHit(RaycastHit hit, CameraContainer c) {
        return hit.point + c.Tuning.Collision_ContactOffset * hit.normal;
    }
}

}