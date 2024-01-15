using System;
using UnityEngine;

namespace ThirdPerson {

[Serializable]
sealed class CameraCollisionSystem: CameraSystem {
    // -- props --
    /// storage for raycasts
    RaycastHit m_Hit;

    /// the pos of the current hit surface
    Vector3 m_HitPos;

    /// the normal of the current hit surface
    Vector3 m_HitNormal;

    // -- System --
    protected override Phase InitInitialPhase() {
        return Tracking;
    }

    public override void Init() {
        base.Init();

        // set initial state
        m_State.Next.Pos = m_State.IntoIdealPosition();
    }

    // -- Tracking --
    Phase Tracking => new(
        name: "Tracking",
        update: Tracking_Update
    );

    void Tracking_Update(float delta) {
        if (m_State.IsFreeLook) {
            ChangeToImmediate(FreeLook, delta);
            return;
        }

        var ideal = m_State.IntoIdealPosition();
        var corrected = GetTrackingPos(ideal);
        m_State.Next.Pos = ideal;

        if (ideal != corrected) {
            ChangeTo(Tracking_Correcting);
            return;
        }
    }

    // -- Tracking_Correcting --
    Phase Tracking_Correcting => new(
        name: "Tracking_Correcting",
        update: Tracking_Correcting_Update
    );

    void Tracking_Correcting_Update(float delta) {
        if (m_State.IsFreeLook) {
            ChangeToImmediate(FreeLook, delta);
            return;
        }

        var idealPos = m_State.IntoIdealPosition();
        var correctPos = GetTrackingPos(idealPos);

        var nextPos = Vector3.MoveTowards(
            m_State.Curr.Pos,
            correctPos,
            m_Tuning.Collision_Tracking_CorrectionSpeed * delta
        );

        m_State.Next.Pos = nextPos;

        if (nextPos == idealPos) {
            ChangeTo(Tracking);
            return;
        }
    }

    // -- FreeLook --
    Phase FreeLook => new(
        name: "FreeLook",
        update: FreeLook_Update
    );

    void FreeLook_Update(float delta) {
        if (!m_State.IsFreeLook) {
            ChangeToImmediate(Tracking, delta);
            return;
        }

        var ideal = m_State.IntoIdealPosition();
        var corrected = GetFreeLookPos(ideal);

        m_State.Next.Pos = ideal;

        if (ideal != corrected) {
            ChangeTo(FreeLook_Colliding);
            return;
        }
    }

    // -- FreeLook_Colliding --
    Phase FreeLook_Colliding => new(
        name: "FreeLook_Colliding",
        update: FreeLook_Colliding_Update
    );

    void FreeLook_Colliding_Update(float delta) {
        if (!m_State.IsFreeLook) {
            ChangeToImmediate(Tracking, delta);
            return;
        }

        var ideal = m_State.IntoIdealPosition();
        var corrected = GetFreeLookPos(ideal);

        // m_State.Next.Pos = m_State.Curr.Pos + (inputImpulse + correctionImpulse);
        if (ideal == corrected) {
            ChangeToImmediate(FreeLook, delta);
            return;
        }

        // scale tolerance with hit normal
        var normalDotUp = Vector3.Dot(m_HitNormal, Vector3.up);
        var tolerance = m_Tuning.Collision_ClipToleranceByNormal.Evaluate(normalDotUp);
        var mag = Vector3.Magnitude(ideal - corrected);
        if (mag > tolerance) {
            ChangeToImmediate(FreeLook_Clipping, delta);
            return;
        }

        // interpolate towards corrected position while colliding
        m_State.Next.Pos = Vector3.MoveTowards(
            m_State.Next.Pos,
            corrected,
            m_Tuning.Collision_FreeLook_CorrectionSpeed * delta
        );
    }

    // -- FreeLook_Clipping--
    Phase FreeLook_Clipping => new(
        name: "FreeLook_Clipping",
        enter: FreeLook_Clipping_Enter,
        update: FreeLook_Clipping_Update
    );

    void FreeLook_Clipping_Enter() {
        m_State.Next.Velocity *= 1.0f - m_Tuning.Collision_ClipDamping.Evaluate(PhaseStart, releaseStartTime: PhaseStart);
    }

    void FreeLook_Clipping_Update(float delta) {
        if (!m_State.IsFreeLook) {
            ChangeToImmediate(Tracking, delta);
            return;
        }

        var ideal = m_State.IntoIdealPosition();
        var corrected = GetFreeLookPos(ideal);

        m_State.Next.Pos = ideal;
        m_State.Next.Velocity *= 1.0f - m_Tuning.Collision_ClipDamping.Evaluate(PhaseStart, releaseStartTime: PhaseStart);

        if (ideal == corrected) {
            ChangeTo(FreeLook_ClippingCooldown);
            return;
        }
    }

    Phase FreeLook_ClippingCooldown => new(
        name: "FreeLook_ClippingCooldown",
        update: FreeLook_ClippingCooldown_Update
    );

    void FreeLook_ClippingCooldown_Update(float delta) {
        if (!m_State.IsFreeLook) {
            ChangeToImmediate(Tracking, delta);
            return;
        }

        var ideal = m_State.IntoIdealPosition();
        var corrected = GetFreeLookPos(ideal);

        m_State.Next.Pos = ideal;

        if (ideal != corrected) {
            ChangeTo(FreeLook_Clipping);
            return;
        }

        if (PhaseElapsed >= m_Tuning.Collision_ClipCooldown) {
            ChangeTo(FreeLook);
            return;
        }
    }

    // -- queries --
    Vector3 GetFreeLookPos(Vector3 candidate) {
        // the final position
        var destPos = candidate;

        // the character's position
        var origin = m_State.FollowPosition;

        // step 1: cast from the character to the ideal position to see if any
        // surface is blocking visibility; use a sphere cast so we don't get
        // closer than the contact offset
        var vizCastStart = candidate - origin;
        var vizLen = vizCastStart.magnitude;
        var vizDir = vizCastStart.normalized;

        var didHit = Physics.SphereCast(
            origin,
            m_Tuning.Collision_ContactOffset,
            vizDir,
            out m_Hit,
            vizLen,
            m_Tuning.Collision_Mask,
            QueryTriggerInteraction.Ignore
        );

        // TODO: don't set state in here
        m_State.Next.IsColliding = didHit;
        m_HitPos = m_Hit.point;
        m_HitNormal = m_Hit.normal;

        // if the target is visible, we have our desired position
        if (!didHit) {
            return destPos;
        }

        // otherwise, we found the point on this surface (note: offset by c.o.
        // so that step 3b works)
        var vizNormal = m_Hit.normal;
        var vizPos = OffsetHit(m_Hit);

        destPos = vizPos;

        // step 2: project the candidate along the plane of the hit surface
        // using the remaining distance to candidate

        // scale the projection down if the pitch is < 0 so that we can pan
        // into the character
        // TODO: add a tuning to scale this differently
        var projK = 1.0f;
        var pitch = m_State.Next.Spherical.Zenith;
        if (pitch < 0.0f) {
            projK = 1.0f - pitch / m_Tuning.FreeLook_MinPitch;
        }

        var projLen = Vector3.Distance(candidate, vizPos);
        var projDir = Vector3.Cross(vizNormal, Vector3.Cross(vizDir, vizNormal)).normalized;
        var projPos = vizPos + projK * projLen * projDir;

        destPos = projPos;

        return destPos;
    }

    /// correct camera position in attempt to preserve line of sight
    /// see: https://miro.com/app/board/uXjVOWfpI6I=/?moveToWidget=3458764535240497690&cot=14
    private Vector3 GetTrackingPos(Vector3 candidate) {
        // the final position
        var destPos = candidate;

        // the character's position
        var origin = m_State.FollowPosition;

        // step 1: cast from the character to the ideal position to see if any
        // surface is blocking visibility; use a sphere cast so we don't get
        // closer than the contact offset
        var vizCastStart = candidate - origin;
        var vizLen = vizCastStart.magnitude;
        var vizDir = vizCastStart.normalized;

        var didHit = Physics.SphereCast(
            origin,
            m_Tuning.Collision_ContactOffset,
            vizDir,
            out m_Hit,
            vizLen,
            m_Tuning.Collision_Mask,
            QueryTriggerInteraction.Ignore
        );

        // TODO: don't set state in here
        m_State.Next.IsColliding = didHit;
        m_HitPos = m_Hit.point;
        m_HitNormal = m_Hit.normal;

        // if the target is visible, we have our desired position
        if (!didHit) {
            return destPos;
        }

        // otherwise, we found the point on this surface (note: offset by c.o.
        // so that step 3b works)
        var vizNormal = m_Hit.normal;
        var vizPos = OffsetHit(m_Hit);

        destPos = vizPos;

        // step 2: project the candidate along the plane of the hit surface
        // using the remaining distance to candidate

        // scale the projection down if the pitch is < 0 so that we can pan
        // into the character
        var projK = 1.0f;
        var pitch = m_State.Next.Spherical.Zenith;
        if (pitch < 0.0f) {
            projK = 1.0f - pitch / m_Tuning.FreeLook_MinPitch;
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
            out m_Hit,
            exitVertLen,
            m_Tuning.Collision_Mask,
            QueryTriggerInteraction.Ignore
        );

        if (didHit) {
            destPos = OffsetHit(m_Hit);
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
                out m_Hit,
                m_Tuning.Collision_Mask,
                QueryTriggerInteraction.Ignore
            );

            if (didHit) {
                destPos = OffsetHit(m_Hit);
            }
        }

        // step 4: do a final vision cast from the player to make sure the destination
        // point is visible.
        var vizCastEndSrc = origin;
        var vizCastEndDst = destPos;

        didHit = Physics.Linecast(
            vizCastEndSrc,
            vizCastEndDst,
            out m_Hit,
            m_Tuning.Collision_Mask,
            QueryTriggerInteraction.Ignore
        );

        if (didHit) {
            destPos = OffsetHit(m_Hit);
        }

        return destPos;
    }

    /// the hit point adjusted by the contact offset
    Vector3 OffsetHit(RaycastHit hit) {
        return hit.point + m_Tuning.Collision_ContactOffset * hit.normal;
    }

    // -- queries --
    /// the pos of the current hit surface
    public Vector3 ClipPos {
        get => m_State.Next.IsColliding ? m_HitPos : m_State.Next.Pos;
    }

    /// the normal of the current hit surface
    public Vector3 ClipNormal {
        get => m_State.Next.IsColliding ? m_HitNormal : m_State.Next.Forward;
    }
}

}