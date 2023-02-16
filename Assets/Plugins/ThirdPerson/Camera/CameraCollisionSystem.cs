using UnityEngine;
using UnityEngine.InputSystem;

namespace ThirdPerson {

sealed class CameraCollisionSystem: System {
    // -- deps --
    /// the current camera state
    CameraState m_State;

    /// .
    CameraTuning m_Tuning;

    // -- props --
    /// the state-machine's state
    SystemState m_SystemState;

    /// storage for raycasts
    RaycastHit m_Hit;

    // -- lifetime --
    public CameraCollisionSystem(
        CameraState state,
        CameraTuning tuning
    ) {
        // set deps
        m_State = state;
        m_Tuning = tuning;
    }

    // -- System --
    protected override SystemState State {
        get => m_SystemState;
        set => m_SystemState = value;
    }

    protected override Phase InitInitialPhase() {
        return Tracking;
    }

    public override void Init() {
        base.Init();

        // set initial state
        m_State.Next.Pos = m_State.IntoPosition();
    }


    // -- Tracking --
    Phase Tracking => new Phase(
        name: "Tracking",
        update: Tracking_Update
    );

    void Tracking_Update(float delta) {
        if (!m_State.IsTracking) {
            ChangeToImmediate(FreeLook, delta);
            return;
        }

        m_State.Next.Pos = GetCorrectedPos(m_State.IntoPosition());
    }

    // -- FreeLook --
    Phase FreeLook => new Phase(
        name: "FreeLook",
        update: FreeLook_Update
    );

    void FreeLook_Update(float delta) {
        if (m_State.IsTracking) {
            ChangeToImmediate(Tracking, delta);
            return;
        }

        m_State.Next.Pos = GetCorrectedPos(m_State.IntoPosition());
    }

    // -- queries --
    /// correct camera position in attempt to preserve line of sight
    /// see: https://miro.com/app/board/uXjVOWfpI6I=/?moveToWidget=3458764535240497690&cot=14
    private Vector3 GetCorrectedPos(Vector3 candidate) {
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
            m_Tuning.ContactOffset,
            vizDir,
            out m_Hit,
            vizLen,
            m_Tuning.CollisionMask,
            QueryTriggerInteraction.Ignore
        );

        m_State.Next.IsColliding = didHit;

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
            m_Tuning.CollisionMask,
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
                m_Tuning.CollisionMask,
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
            m_Tuning.CollisionMask,
            QueryTriggerInteraction.Ignore
        );

        if (didHit) {
            destPos = OffsetHit(m_Hit);
        }

        return destPos;
    }

    /// the hit point adjusted by the contact offset
    Vector3 OffsetHit(RaycastHit hit) {
        return hit.point + m_Tuning.ContactOffset * hit.normal;
    }
}

}