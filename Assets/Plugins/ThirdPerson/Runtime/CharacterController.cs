using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThirdPerson {

/// a re-implementation of unity's built-in character cntrlr w/ better
/// collision handling
[Serializable]
public sealed class CharacterController {
    /// .
    public struct Frame {
        /// .
        public Vector3 Position;

        /// .
        public Vector3 Velocity;

        /// the difference between input velocity and the frame velocity
        public Vector3 Inertia;

        /// .
        public CharacterCollision Wall;

        /// .
        public CharacterCollision Ground;

        /// .
        public Buffer<CharacterCollision> Surfaces;

        // -- lifetime --
        public Frame(uint maxCollisions) {
            Position = Vector3.zero;
            Velocity = Vector3.zero;
            Inertia = Vector3.zero;
            Wall = CharacterCollision.None;
            Ground = CharacterCollision.None;
            Surfaces = new Buffer<CharacterCollision>(maxCollisions);
        }

        // -- commands --
        /// add a new surface to the buffer
        public void AddSurface(CharacterCollision surface) {
            Surfaces.Add(surface);
        }
    }

    // why are we not using rigidbodies? they are annoying to work around. since
    // we have a bunch of custom, unrealistic collision physics, we might as
    // well implement it with full control. the best thing we would get from
    // rigidbodies is the possibility of other objects colliding with the
    // character, we can implement that as an add-on/child

    // -- constants --
    /// the max number of casts we do in a single frame
    const int k_MaxCasts = 4;

    /// the max number of collisions we check for at the end of the frame
    const int k_MaxCollisions = 4;

    // -- fields --
    [Header("config")]
    [Tooltip("the collision mask for the character")]
    [SerializeField] LayerMask m_CollisionMask;

    [Tooltip("the minimum move delta to have any effect")]
    [SerializeField] float m_MinMove;

    [Tooltip("the minimum speed to have any effect")]
    [SerializeField] float m_MinSpeed;

    [Tooltip("the highest angle in which colliding with is considered ground. ie slope angle")]
    [SerializeField] float m_WallAngle;

    [Tooltip("the amount to offset collision casts against the movement to avoid precision issues")]
    [SerializeField] float m_CastOffset;

    [Tooltip("the amount to offset movement from collisions")]
    [SerializeField] float m_ContactOffset;

    [Tooltip("a small amount over contact offset to detect hits")]
    [SerializeField] float m_ContactEpsilon;

    [Tooltip("the extra amount to search for nearby colliders")]
    [SerializeField] float m_ContactSearch;

    [Header("refs")]
    [Tooltip("the character's capsule")]
    [SerializeField] CapsuleCollider m_Capsule;

    // -- props --
    /// the list of colliders in contact offset range at the end of the frame
    Collider[] m_Colliders;

    /// the character's capsule collider w/ the contact offset integrated into its radius
    CapsuleCollider m_CapsuleWithSearch;

    /// the square min move magnitude
    float m_SqrMinMove;

    /// the square min speed
    float m_SqrMinSpeed;

    // -- debug --
    #if UNITY_EDITOR
    /// the start position
    Vector3 m_DebugMoveOrigin;

    /// the delta
    Vector3 m_DebugMoveDelta;

    /// the last collision hit
    List<RaycastHit> m_DebugHits = new();

    /// the list of casts this frame
    List<Capsule.Cast> m_DebugCasts = new();

    /// a bad hit, an error
    RaycastHit? m_DebugErrorHit = null;
    #endif

    // -- commands --
    /// initialize the controller
    public void Init() {
        // set props
        m_Colliders = new Collider[k_MaxCollisions];
        m_SqrMinMove = m_MinMove * m_MinMove;
        m_SqrMinSpeed = m_MinSpeed * m_MinSpeed;

        // clone collider & add contact offset for compute penetration
        var src = m_Capsule;
        var dst = m_Capsule.gameObject.AddComponent<CapsuleCollider>();
        dst.center = src.center;
        dst.height = src.height;
        dst.radius = src.radius + m_ContactOffset + m_ContactSearch;
        dst.direction = src.direction;
        dst.isTrigger = true;

        m_CapsuleWithSearch = dst;
    }

    /// move the character by a position delta
    public Frame Move(
        Vector3 pos,
        Vector3 velocity,
        Vector3 up,
        float delta
    ) {
        // the slice of our delta time remaining to resolve
        var timeRemaining = delta;

        // the final velocity at the end of the move
        var nextVelocity = velocity;

        // the move's original position
        var moveOrigin = pos;

        // the current submove
        var moveSrc = moveOrigin;
        var moveDst = moveSrc;

        // store debug move
        #if UNITY_EDITOR
        m_DebugMoveDelta = velocity * delta;
        m_DebugMoveOrigin = moveOrigin;
        #endif

        // pre-calculate cast capsule
        var c = m_Capsule;
        var capsule = new Capsule(
            c.center,
            c.radius,
            c.height,
            up
        );

        // buffered storage
        var didHit = false;
        var hit = new RaycastHit();
        var hitDir = Vector3.zero;
        var hitDist = 0f;

        // prepare a new frame
        var nextFrame = new Frame(k_MaxCollisions);

        // DEBUG: reset state
        #if UNITY_EDITOR
        m_DebugCasts.Clear();
        m_DebugHits.Clear();
        #endif

        // while there is any more to move, process what's left of the move
        // delta as a submove
        //
        // ---------------
        // -- 0. guards --
        // ---------------
        // *check to see if we can execute this submove*
        //
        // 0a. if the submove is too small, accumulate it for next frame 0b. if
        //     we've cast too many times, we're probably stuck in a corner. drop the
        //     move entirely and reset to input position
        //
        // -------------
        // -- 1. cast --
        // -------------
        // *search for collisions*
        //
        // 1a. cast from the end of the previous move w/ an offset to avoid
        //     starting inside collision surfaces and missing 1b. if a miss,
        //     that means there's nothing to collide with and we can move to the
        //     end of our cast, we're done! 1c. if hit, then find the center of
        //     the capsule at the hit point, we'll start the next submove from
        //     (around) there.
        //
        // ----------------
        // -- 2. prepare --
        // ----------------
        // *adjust the next submove*
        //
        // 2a. add contact offset to move delta; this is a skin that causes us
        //     to always hover away from surfaces. we cast at least this amount
        //     into the surface, and then remove it once we find the hit. 2b. if
        //     this submove was too small, then accumulate it, for the next
        //     iteration and make the next submove from the same position as
        //     this one, instead of the hit point 2c. determine the remaining
        //     move by projecting the move into the collision surface 2d. track
        //     collisions
        //

        var numCasts = 0;
        while (timeRemaining > 0f) {
            // move delta is however far we can move in the time slice
            var moveDelta = nextVelocity * timeRemaining;

            // if move remaining is less than min move, stop & add it to pending delta
            if (moveDelta.sqrMagnitude <= m_SqrMinMove) {
                // TODO: what to do about any remaining time/velocity/&c
                break;
            }

            // if we cast an unlikely number of times, cancel this move
            // TODO: should moveDelta be zero here?
            if (numCasts > k_MaxCasts) {
                // TODO: what to do about any remaining time
                moveDst = moveOrigin;
                Debug.LogWarning($"[cntrlr] cast more than {k_MaxCasts + 1} times in a single frame!");
                break;
            }

            // this move starts from the previous move's endpoint
            moveSrc = moveDst;

            // decompose the move
            var moveDir = moveDelta.normalized;
            var moveLen = moveDelta.magnitude;

            // get the next capsule cast, offsetting based on move dir
            var castSrc = moveSrc - moveDir * m_CastOffset;
            var castDst = moveSrc + moveDelta;
            var castLen = moveLen + m_CastOffset + m_ContactOffset;
            var cast = capsule.IntoCast(
                castSrc,
                moveDir,
                castLen
            );

            // DEBUG: track cast
            #if UNITY_EDITOR
            m_DebugCasts.Add(cast);
            #endif

            // check for a collision
            didHit = Physics.CapsuleCast(
                cast.Point1,
                cast.Point2,
                cast.Radius,
                cast.Direction,
                out hit,
                cast.Length,
                m_CollisionMask,
                QueryTriggerInteraction.Ignore
            );

            // if we missed, move to the target position
            if (!didHit) {
                moveDst = castDst;
                break;
            }

            // DEBUG: track hit
            #if UNITY_EDITOR
            m_DebugHits.Add(hit);
            #endif

            // move backwards along the move dir to the last point where we were
            // at least contact offset away from the hit surface
            // TODO: is the max right? we can end up < contact offset away now mid move
            var moveOffset = m_ContactOffset / Vector3.Dot(-moveDir, hit.normal);
            var moveMag = Math.Max(hit.distance - moveOffset, 0f);
            moveDst = castSrc + moveMag * moveDir;

            // if displacement is less than min move, try again from the prev
            // position w/ next velocity
            var moveDsp = moveDst - moveSrc;
            if (moveDsp.sqrMagnitude < m_SqrMinMove) {
                moveDst = moveSrc;
            }

            // calculate the remaining movement in the plane
            // TODO: ProjectOnPlane could be a slope function e.g. mR.dir * fn(mR.dir • N) * mR.mag
            var moveRemaining = castDst - moveDst;

            // assuming velocity is constant during this period of time:
            timeRemaining = moveRemaining.magnitude / nextVelocity.magnitude;
            nextVelocity = Vector3.ProjectOnPlane(nextVelocity, hit.normal);

            // update state
            numCasts++;
        }

        // zero out small speed
        if (nextVelocity.sqrMagnitude <= m_SqrMinSpeed) {
            nextVelocity = Vector3.zero;
        }

        // update frame velocity
        nextFrame.Velocity = nextVelocity;
        nextFrame.Inertia = velocity - nextVelocity;

        // find any colliders we're contact offset away from
        var capsulePts = capsule.Offset(moveDst).Points();
        var numOverlaps = Physics.OverlapCapsuleNonAlloc(
            capsulePts.point1,
            capsulePts.point2,
            m_CapsuleWithSearch.radius,
            m_Colliders,
            m_CollisionMask
        );

        // accumulate an offset to ensure we're contact offset away from every collider
        var collisionOffset = Vector3.zero;

        // find collisions with each nearby collider
        for (var i = 0; i < numOverlaps; i++) {
            // check for a collision
            var collider = m_Colliders[i];

            // get next capsule cast towards collided surface
            var castSrc = moveDst;
            var castDir = Vector3.zero;
            var castMax = m_ContactOffset + m_ContactEpsilon;
            var castLen = m_ContactOffset + m_ContactSearch;
            var castRes = CastResult.Miss;

            // we can only call closest point on convex colliders
            var colliderSupportsClosestPoint = (
                (collider is not TerrainCollider) &&
                (collider is not MeshCollider m || m.convex)
            );

            // if this is a convex collider
            if (colliderSupportsClosestPoint) {
                castDir = collider.ClosestPoint(moveDst) - castSrc;

                castRes = CollideCapsule(
                    collider,
                    capsule,
                    castSrc,
                    castDir,
                    castMax,
                    castLen,
                    ref hit,
                    ref collisionOffset,
                    ref nextFrame
                );

                if (castRes == CastResult.Miss) {
                    Debug.LogWarning($"[cntrlr] final collision cast missed convex mesh {collider}");
                }
            }
            // otherwise, depenetrate from concave mesh to find dir
            else {
                var colliderTransform = collider.transform;
                didHit = Physics.ComputePenetration(
                    m_CapsuleWithSearch,
                    moveDst,
                    Quaternion.identity,
                    collider,
                    colliderTransform.position,
                    colliderTransform.rotation,
                    out hitDir,
                    out hitDist
                );

                if (!didHit) {
                    Debug.LogWarning($"[cntrlr] depenetration missed for {collider}");
                    continue;
                }

                // move cast back by hit distance; increase cast length by search radius to account
                // for difference between capsule & search capsule radii
                castSrc += hitDist * hitDir;
                castDir = -hitDir;
                castMax += hitDist + m_ContactSearch;

                // cast along the surface to find all collision surfaces on the concave mesh
                for (numCasts = 0; numCasts < k_MaxCasts; numCasts++) {
                    castRes = CollideCapsule(
                        collider,
                        capsule,
                        castSrc,
                        castDir,
                        castMax,
                        castLen: castMax,
                        ref hit,
                        ref collisionOffset,
                        ref nextFrame
                    );

                    if (castRes == CastResult.Miss) {
                        if (numCasts == 0) {
                            Debug.LogWarning($"[cntrlr] final collision cast missed concave mesh {collider}");
                        }

                        break;
                    }

                    // project cast direction into the surface normal
                    var nextDir = Vector3.ProjectOnPlane(castDir, hit.normal);
                    var nextMag = nextDir.magnitude;

                    // project how much is left from our maximum cast, if any
                    castMax *= nextMag;
                    if (castMax <= 0f) {
                        break;
                    }

                    // start next cast from the hit point
                    castDir = nextDir / nextMag;
                }
            }
        }

        // apply offset to depenetrate from colliders
        moveDst += collisionOffset;

        // update final frame position
        nextFrame.Position = moveDst;

        return nextFrame;
    }

    enum CastResult {
        Hit,
        Miss,
        OutOfRange
    }

    CastResult CollideCapsule(
        Collider collider,
        Capsule capsule,
        Vector3 castSrc,
        Vector3 castDir,
        float castMax,
        float castLen,
        ref RaycastHit hit,
        ref Vector3 offset,
        ref Frame nextFrame
    ) {
        // find the contact point on the surface
        var cast = capsule.IntoCast(
            castSrc,
            castDir.normalized,
            castLen
        );

        var didHit = Physics.CapsuleCast(
            cast.Point1,
            cast.Point2,
            cast.Radius,
            cast.Direction,
            out hit,
            cast.Length,
            m_CollisionMask,
            QueryTriggerInteraction.Ignore
        );

        // this has to hit the same collider
        if (!didHit || hit.collider != collider) {
            return CastResult.Miss;
        }

        // ignore hits farther away than the offset but within search radius
        if (hit.distance > castMax) {
            return CastResult.OutOfRange;
        }

        // accumulate the offset
        offset += Mathf.Max(m_ContactOffset - hit.distance, 0f) * hit.normal;

        // track collisions
        var collisionAngle = Vector3.Angle(hit.normal, Vector3.up);
        var collisionNormalMag = Mathf.Max(Vector3.Dot(hit.normal, -nextFrame.Inertia), 0f);
        var collision = new CharacterCollision(
            hit.normal,
            hit.point,
            collisionAngle,
            collisionNormalMag
        );

        // track wall & ground collision separately for external querying
        if (collision.Angle >= m_WallAngle) {
            nextFrame.Wall = collision;
        } else {
            nextFrame.Ground = collision;
        }

        nextFrame.AddSurface(collision);

        return CastResult.Hit;
    }

    // -- queries --
    /// the controller's contact offset
    public float ContactOffset {
        get => m_ContactOffset;
    }

    // -- gizmos --
    #if UNITY_EDITOR
    // -- gizmos --
    [Header("gizmos")]
    [Tooltip("if the initial position and direction gizmo is visible")]
    [SerializeField] bool m_DrawInput = true;

    [Tooltip("if we are drawing the top sphere of the capsule")]
    [SerializeField] bool m_DrawTop = true;

    [Tooltip("if we are drawing the the capsule as wireframe")]
    [SerializeField] bool m_DrawWire = true;

    [Tooltip("if the raycasts gizmos are visible")]
    [SerializeField] bool m_DrawCasts = true;

    [Tooltip("if the raycasts gizmos are visible")]
    [SerializeField] bool m_DrawCastCapsule = true;

    [Tooltip("if the cast hit gizmos are visible")]
    [SerializeField] bool m_DrawHits = true;

    [Tooltip("the radius of the gizmo spheres")]
    [SerializeField] float m_DrawGizmoRadius;

    /// draw gizmos for the controller`
    public void OnDrawGizmos() {
        // draw the desired ray (pos & delta)
        if (m_DrawInput) {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(m_DebugMoveOrigin, m_DrawGizmoRadius);
            Gizmos.DrawRay(m_DebugMoveOrigin, m_DebugMoveDelta);
        }

        // draw the cast lollipops
        if (m_DrawCasts) {
            Color.RGBToHSV(Color.blue, out var iH, out var iS, out var iV);
            Color.RGBToHSV(Color.red, out var oH, out var oS, out var oV);

            for (var i = m_DebugCasts.Count - 1; i >= 0; i--) {
                var cast = m_DebugCasts[i];

                var h = cast.Radius * Vector3.up;
                var delta = cast.Direction * cast.Length;

                Gizmos.color = Color.HSVToRGB(iH, iS, iV);
                if (m_DrawCastCapsule) {
                    var point = m_DrawTop ? cast.Point2 : cast.Point1;
                    Action<Vector3, float> drawSphere = m_DrawWire ? Gizmos.DrawWireSphere : Gizmos.DrawSphere;
                    drawSphere(point, cast.Radius);
                    Gizmos.DrawLine(cast.Point1 - h, cast.Point2 + h);
                }

                Gizmos.color = Color.HSVToRGB(oH, oS, oV);
                if (m_DrawCastCapsule) {
                    var point = m_DrawTop ? cast.Point2 : cast.Point1;
                    Action<Vector3, float> drawSphere = m_DrawWire ? Gizmos.DrawWireSphere : Gizmos.DrawSphere;
                    drawSphere(point + delta, cast.Radius);
                    Gizmos.DrawLine(cast.Point1 - h + delta, cast.Point2 + h + delta);
                }

                // draw the final line
                Gizmos.DrawSphere(cast.Capsule.Center, m_DrawGizmoRadius);
                Gizmos.DrawLine(cast.Capsule.Center, cast.Capsule.Center + delta);
                iS *= 0.6f;
                oS *= 0.6f;
            }
        }

        // draw spheres where the casts hit
        if (m_DrawHits) {
            Color.RGBToHSV(Color.yellow, out var h, out var s, out var v);

            foreach (var hit in m_DebugHits) {
                Gizmos.color = Color.HSVToRGB(h, s, v);
                Gizmos.DrawSphere(hit.point, m_DrawGizmoRadius);
                Gizmos.DrawRay(hit.point, hit.normal * 0.5f);
                s *= 0.6f;
            }

            if (m_DebugErrorHit != null) {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(m_DebugErrorHit.Value.point, 0.5f);
            }
        }
    }
    #endif
}
}