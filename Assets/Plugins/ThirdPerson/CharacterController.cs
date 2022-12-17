using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThirdPerson {

/// a re-implementation of unity's built-in character cntrlr w/ better
/// collision handling
[Serializable]
public sealed class CharacterController {
    public struct Frame {
        public Vector3 Position;
        public Vector3 Velocity;
        public CharacterCollision Wall;
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

    [Tooltip("the minimum move vector to have any effect")]
    [SerializeField] float m_MinMove;

    [Tooltip("the highest angle in which colliding with is considered ground. ie slope angle")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_MaxGroundAngle")]
    [SerializeField] float m_WallAngle;

    [Tooltip("the amount to offset collision casts against the movement to avoid precision issues")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_CastDirOffset")]
    [SerializeField] float m_CastOffset;

    [Tooltip("the amount to offset TODO:...?????")]
    [SerializeField] float m_ContactOffset;

    [Header("refs")]
    [Tooltip("the character's capsule")]
    [SerializeField] CapsuleCollider m_Capsule;

    // -- props --
    /// the square min move magnitude
    float m_SqrMinMove;

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
    }

    /// move the character by a position delta
    public Frame Move(
        Vector3 pos,
        Vector3 velocity,
        Vector3 up,
        float deltaTime
    ) {
        // the slice of our delta time remaining to resolve
        var timeLeft = deltaTime;

        // the final velocity at the end of the move
        var nextVelocity = velocity;

        // the move's original position
        var moveOrigin = pos;

        // the current submove
        var moveSrc = moveOrigin;
        var moveDst = moveSrc;

        // store debug move
        #if UNITY_EDITOR
        m_DebugMoveDelta = velocity * deltaTime;
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
        while (timeLeft > 0f) {
            // asdfasdf
            if (nextVelocity == Vector3.zero) {
                break;
            }

            // move delta is however far we can move in the time slice
            var moveDelta = nextVelocity * timeLeft;

            // if move remaining is less than min move, stop & add it to pending delta
            if (moveDelta.sqrMagnitude <= m_SqrMinMove) {
                // TODO: what to do about any remaining time/velocity/8c
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

            // get the next capsule cast, offsetting based on move dir
            var castSrc = moveSrc - moveDelta.normalized * m_CastOffset;
            var castDst = moveSrc + moveDelta;
            var castDelta = castDst - castSrc;
            var cast = capsule.IntoCast(
                castSrc,
                castDelta.normalized,
                castDelta.magnitude + m_ContactOffset
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

            // hit.distance is the length of the ray, so it's at the capsulte center
            var hitCapsuleCenter = castSrc + hit.distance * cast.Direction;

            // add offset away from hit plane
            var moveActual = hitCapsuleCenter - moveSrc;
            moveDst = hitCapsuleCenter - nextVelocity.normalized * Mathf.Min(m_ContactOffset, moveActual.magnitude);

            // if displacement is less than min move, try again from the prev
            // position w/ next velocity
            var moveDsp = moveDst - moveSrc;
            if (moveDsp.sqrMagnitude < m_SqrMinMove) {
                moveDst = moveSrc;
            }

            // calculate the remaining movement in the plane
            // TODO: ProjectOnPlane could be a slope function e.g. mR.dir * fn(mR.dir • N) * mR.mag
            var moveRemaining = castDst - moveDst;

            // assuming velocity is constant during this period of time:
            timeLeft = moveRemaining.magnitude / nextVelocity.magnitude;

            var projectedVelocity = Vector3.ProjectOnPlane(nextVelocity, hit.normal);

            // if the angle between our raw remaining move and the move in the plane
            // are pependicular-ish, don't move along the surface
            var moveAngle = Vector3.Angle(nextVelocity, projectedVelocity);
            if (moveAngle >= m_WallAngle) {
                projectedVelocity = Vector3.zero;
            }

            nextVelocity = projectedVelocity;

            // track collisions
            var collision = new CharacterCollision(
                hit.normal,
                hit.point
            );

            // track wall & ground collision separately, both for external
            // querying and for determining our cast offset
            if (Vector3.Angle(hit.normal, Vector3.up) > m_WallAngle) {
                nextWall = collision;
            } else {
                nextGround = collision;
            }

            // update state
            i++;
        }

        return new Frame() {
            Position = moveDst,
            Velocity =  nextVelocity,
            Wall = nextWall,
            Ground = nextGround,
        };
    }

    // -- queries --
    /// the angle considered a wall
    public float WallAngle {
        get => m_WallAngle;
    }

    // -- gizmos --
    #if UNITY_EDITOR
    /// the radius
    const float k_DebugGizmoRadius = 0.15f;

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

    /// draw gizmos for the controller`
    public void OnDrawGizmos() {
        // draw the desired ray (pos & delta)
        if (m_DrawInput) {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(m_DebugMoveOrigin, k_DebugGizmoRadius);
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
                Gizmos.DrawSphere(cast.Capsule.Center, k_DebugGizmoRadius*oS);
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
                Gizmos.DrawSphere(hit.point, k_DebugGizmoRadius);
                Gizmos.DrawRay(hit.point, hit.normal * 0.5f);
                s *= 0.6f;
            }

            if (m_DebugErrorHit != null) {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(m_DebugErrorHit.Value.point, 0.5f);
            }
        }

        // // draw the final position
        // Gizmos.color = Color.cyan;
        // Gizmos.DrawSphere(m_Position, k_DebugGizmoRadius);

        // // draw labels
        // UnityEditor.Handles.color = Color.yellow;
        // UnityEditor.Handles.Label(
        //     m_Position - Quaternion.AngleAxis(90.0f, Vector3.up) * m_Velocity.normalized * 0.3f,
        //     $"casts: {m_DebugCasts.Count} hits: {m_DebugHits.Count}"
        // );
    }
    #endif
}
}