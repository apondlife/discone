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

        /// .
        public CharacterCollision Wall;

        /// .
        public CharacterCollision Ground;
    }

    // why are we not using rigidbodies? they are annoying to work around. since
    // we have a bunch of custom, unrealistic collision physics, we might as
    // well implement it with full control. the best thing we would get from
    // rigidbodies is the possibility of other objects colliding with the
    // character, we can implement that as an add-on/child

    // -- constants --
    /// the max amount of casts we do in a single frame
    const int k_MaxCasts = 4;

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

    [Tooltip("the amount to offset TODO:...?????")]
    [SerializeField] float m_ContactOffset;

    [Header("refs")]
    [Tooltip("the character's capsule")]
    [SerializeField] CapsuleCollider m_Capsule;

    // -- props --
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
    List<RaycastHit> m_DebugHits = new List<RaycastHit>();

    /// the list of casts this frame
    List<Capsule.Cast> m_DebugCasts = new List<Capsule.Cast>();

    /// a bad hit, an error
    RaycastHit? m_DebugErrorHit = null;
    #endif

    // -- commands --
    /// initialize the controller
    public void Init() {
        // set props
        m_SqrMinMove = m_MinMove * m_MinMove;
        m_SqrMinSpeed = m_MinSpeed * m_MinSpeed;
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

        // clear the collisions
        var nextWall = new CharacterCollision();
        var nextGround = new CharacterCollision();

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


        var i = 0;
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
            if (i > k_MaxCasts) {
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
            var didHit = Physics.CapsuleCast(
                cast.Point1,
                cast.Point2,
                cast.Radius,
                cast.Direction,
                out var hit,
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
            var moveOffset = m_ContactOffset / Vector3.Dot(-moveDir, hit.normal);
            moveDst = castSrc + (hit.distance - moveOffset) * moveDir;

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

            // track collisions
            var collision = new CharacterCollision(
                hit.normal,
                hit.point
            );

            // track wall & ground collision separately for external querying
            if (collision.Angle >= m_WallAngle) {
                nextWall = collision;
            } else {
                nextGround = collision;
            }

            // update state
            i++;
        }

        // zero out small speed
        if (nextVelocity.sqrMagnitude <= m_SqrMinSpeed) {
            nextVelocity = Vector3.zero;
        }

        return new Frame() {
            Position = moveDst,
            Velocity = nextVelocity,
            Wall = nextWall,
            Ground = nextGround,
        };
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