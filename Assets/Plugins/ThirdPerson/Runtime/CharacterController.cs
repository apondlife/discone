using System;
using Soil;
using UnityEngine;
using Color = UnityEngine.Color;

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

        /// .
        public Buffer<CharacterCollision> Surfaces;

        // -- lifetime --
        public Frame(uint maxCollisions) {
            Position = Vector3.zero;
            Velocity = Vector3.zero;
            Surfaces = new Buffer<CharacterCollision>(maxCollisions);
        }

        // -- commands --
        /// add a new surface to the buffer
        public void AddSurface(CharacterCollision surface) {
            // search for an existing collision by normal
            for (var i = 0; i < Surfaces.Count; i++) {
                var other = Surfaces[i];

                // if found, combine this as a second source
                if (other.Normal == surface.Normal) {
                    other.AddSource(surface.Source, surface.Point);
                    Surfaces[i] = other;

                    // and exit
                    return;
                }
            }

            // if nothing is found, add the new surface
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
    const int k_MaxCasts = 8;

    /// the max number of collisions we check for at the end of the frame
    const int k_MaxCollisions = 8;

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

        //
        // move pass
        //

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

            // if move remaining is less than min move, stop
            if (moveDelta.sqrMagnitude <= m_SqrMinMove) {
                // TODO: what to do about any remaining time/velocity/&c
                break;
            }

            // if we cast an unlikely number of times, cancel this move
            // TODO: should moveDelta be zero here?
            if (numCasts > k_MaxCasts) {
                // TODO: what to do about any remaining time
                moveDst = moveOrigin;
                Log.Controller.W($"cast more than {k_MaxCasts + 1} times in a single frame!");
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

            var surface = new CharacterCollision(
                hit.normal,
                hit.point,
                CollisionSource.Move
            );

            nextFrame.AddSurface(surface);

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

        // update frame velocity for overlap pass
        nextFrame.Velocity = nextVelocity;

        DebugDraw.Push(
            "collision-velocity-post-move",
            moveDst,
            nextFrame.Velocity,
            new DebugDraw.Config(tags: DebugDraw.Tag.Collision)
        );

        //
        // overlap pass
        //

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
            var castSrc = moveDst + collisionOffset;
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
                castDir = collider.ClosestPoint(castSrc) - castSrc;
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
                    Log.Controller.W($"final collision cast convex mesh {collider} missed");
                }

                if (castRes != CastResult.Hit) {
                    // TODO: we should be able to parameterize a value w/ i, castRes, &c
                    DebugDraw.Push(
                        $"collision-overlap/{i}/{castRes}",
                        hit.point,
                        new DebugDraw.Config(GetDebugColor(castRes, CollisionSource.Overlap), width: 3f, tags: DebugDraw.Tag.Collision)
                    );
                }
            }
            // otherwise, depenetrate from concave mesh to find dir
            else {
                var colliderTransform = collider.transform;
                didHit = Physics.ComputePenetration(
                    m_CapsuleWithSearch,
                    castSrc,
                    Quaternion.identity,
                    collider,
                    colliderTransform.position,
                    colliderTransform.rotation,
                    out hitDir,
                    out hitDist
                );

                if (!didHit) {
                    Log.Controller.W($"depenetration missed for {collider}");
                    continue;
                }

                // move cast back by hit distance; increase cast length by search radius to account
                // for difference between capsule & search capsule radii
                castSrc += hitDist * hitDir;
                castDir = -hitDir;
                castMax += hitDist;
                castLen += hitDist;

                // cast towards the surface to find all collisions on the concave mesh
                // from the same initial depenetrated point
                for (numCasts = 0; numCasts < k_MaxCasts; numCasts++) {
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

                    // TODO: should this be worried about out of range cases?
                    // since we are testing all the surface around the character, we should still try and collide with the new projected direction
                    if (castRes == CastResult.Miss) {
                        if (numCasts == 0) {
                            Log.Controller.W($"final collision cast missed concave mesh {collider}");
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

                    // normalize cast dir for next loop
                    castDir = nextDir / nextMag;
                }
            }
        }

        // apply offset to depenetrate from colliders
        moveDst += collisionOffset;

        // update final frame position
        nextFrame.Position = moveDst;

        // zero out small speed
        if (nextFrame.Velocity.sqrMagnitude <= m_SqrMinSpeed) {
            nextFrame.Velocity = Vector3.zero;
        }

        // debug drawings
        for (var i = 0; i < nextFrame.Surfaces.Count; i++) {
            var surface = nextFrame.Surfaces[i];
            DebugDraw.Push(
                $"collision-surface/{numCasts}/{surface.Source}",
                surface.Point,
                surface.Normal,
                new DebugDraw.Config(GetDebugColor(CastResult.Hit, surface.Source), width: 3f, tags: DebugDraw.Tag.Collision)
            );
        }

        return nextFrame;
    }

    enum CastResult {
        Hit,
        OutOfRange,
        Blocked,
        Miss
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

        // it has to hit
        if (!didHit) {
            return CastResult.Miss;
        }

        // ...the same collider
        if (hit.collider != collider) {
            return CastResult.Blocked;
        }

        // ignore hits farther away than the offset but within search radius
        if (hit.distance > castMax) {
            return CastResult.OutOfRange;
        }

        // accumulate the offset
        offset += Mathf.Max(m_ContactOffset - hit.distance, 0f) * hit.normal;

        // if moving into the surface, cancel any remaining velocity into it
        var normalSpeed = Vector3.Dot(nextFrame.Velocity, hit.normal);
        if (normalSpeed < 0f) {
            nextFrame.Velocity -= normalSpeed * hit.normal;
        }

        // track collisions
        var surface = new CharacterCollision(
            hit.normal,
            hit.point,
            CollisionSource.Overlap
        );

        nextFrame.AddSurface(surface);

        return CastResult.Hit;
    }

    // -- queries --
    /// the controller's contact offset
    public float ContactOffset {
        get => m_ContactOffset;
    }

    // -- debugging --
    Color GetDebugColor(
        CastResult res,
        CollisionSource src
    ) {
        return (res, src) switch {
            (CastResult.Blocked, _) => Color.yellow,
            (CastResult.OutOfRange, _) => Color.blue,
            (CastResult.Hit, CollisionSource.Move) => new Color(0.0f, 1f, 0.7f),
            (CastResult.Hit, CollisionSource.Overlap) => new Color(0.7f, 1f, 0.0f),
            (CastResult.Hit, _) => Color.green,
            _ => Color.red
        };
    }
}

}