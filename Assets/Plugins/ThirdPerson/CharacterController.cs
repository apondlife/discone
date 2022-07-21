using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThirdPerson {

/// a re-implementation of unity's built-in character controller w/ better
/// collision handling
[Serializable]
public sealed class CharacterController {
    // why are we not using rigidbodies? they seem to be more annoying to work around. since
    // we have a bunch of custom unrealistic collision physics, might as well implement it
    // with full control. the best thing we would get from rigidbodies is the possibility of
    // other objects colliding with the character, we can implement that as an add-on/child

    // -- fields --
    [Header("config")]
    [Tooltip("the collision mask for the character")]
    [SerializeField] private LayerMask m_CollisionMask;

    [Tooltip("the minimum move vector to have any effect")]
    [SerializeField] private float m_MinMove;

    [Tooltip("the highest angle in which colliding with is considered ground. ie slope angle")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_MaxGroundAngle")]
    [SerializeField] private float m_WallAngle;

    [Tooltip("the amount to offset collision casts to avoid precision issues")]
    [SerializeField] float m_CastOffset;

    [Tooltip("the amount to offset TODO:...?????")]
    [SerializeField] float m_ContactOffset;

    [Tooltip("the amount of fake gravity that is applied in the first move to maintain the character grounded")]
    [SerializeField] float m_GroundedGravity;

    [Header("refs")]
    [Tooltip("the character's capsule")]
    [SerializeField] CapsuleCollider m_Capsule;

    // -- props --
    /// the character's current velocity
    Vector3 m_Position;

    /// the character's current velocity
    Vector3 m_Velocity;

    /// if the character is touching the ground
    bool m_IsGrounded;

    /// the normal of the last collision surface
    Vector3 m_HitNormal = Vector3.up;

    // how much movement should be lost when hitting a slope
    // parameters an angle from 0 to 1 (0 deg to 90 deg)
    // returns a value between 0 and 1 to multiply the next movement by
    private Func<float, float> CustomSlopeAngleLossFunction = null;

    private float LimitSlopeToWallAngle(float angle) {
        return angle < m_WallAngle ? 1.0f : 0.0f;
    }

    /// the collisions this frame
    Buffer<CharacterCollision> m_Collisions = new Buffer<CharacterCollision>(5);

    /// pending move delta, if there is any
    Vector3 m_PendingMoveDelta;

    // -- debug --
    #if UNITY_EDITOR
    /// the last collision hit
    List<RaycastHit> m_DebugHits = new List<RaycastHit>();
    #endif

    #if UNITY_EDITOR
    /// the list of casts this frame
    List<Capsule.Cast> m_DebugCasts = new List<Capsule.Cast>();
    #endif

    // -- commands --
    /// move the character by a position delta
    public void Move(Vector3 position, Vector3 delta, Vector3 up) {
        // if the move was big enough to fire
        var mag = delta.magnitude;
        if (delta.magnitude <= m_MinMove) {
            // accumulate it
            m_PendingMoveDelta += delta;

            // and once we've accumulated enough, make that move
            if (m_PendingMoveDelta.magnitude > m_MinMove) {
                delta = m_PendingMoveDelta;
                m_PendingMoveDelta = Vector3.zero;
            }
            // until then, stop here
            else {
                Debug.Log($"[controller] move delta {mag} < {m_MinMove}; accumulating");
                return;
            }
        }

        // calculate capsule
        var c = m_Capsule;
        var capsule = new Capsule(
            c.center,
            c.radius,
            c.height,
            up
        );

        // track start and end position to calculate velocity
        var moveStart = position;
        var moveEnd = moveStart;
        var moveDelta = delta;
        var moveContactOffset = Vector3.zero;
        var isGrounded = false;

        // temporary grounded calculation
        // DEBUG: reset state
        #if UNITY_EDITOR
        m_DebugCasts.Clear();
        m_DebugHits.Clear();
        #endif

        // clear the collision buffer
        m_Collisions.Clear();

        // while there is any more to move
        var i = 0;
        while (true) {
            // is this necessary? yes, it happens a lot.
            var moveMag = moveDelta.magnitude;
            if (moveMag <= m_MinMove) {
                break;
            }

            // if we cast an unlikely number of times, stop
            if (i > 5) {
                Debug.LogError("[controller] cast more than 5 times in a single frame!");
                break;
            }

            // capsule cast the remaining move
            var castDir = moveDelta.normalized;
            var castLen = moveMag + m_CastOffset + m_ContactOffset;
            var movePos = moveEnd;
            var castPos = movePos - castDir * m_CastOffset;

            var cast = capsule.IntoCast(
                castPos,
                castDir,
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
            var moveTarget = moveEnd + moveDelta;
            if (!didHit) {
                moveEnd = moveTarget;
                // TODO: if this is the first cast, we need to clear the normal, we're in the air
                break;
            }

            // DEBUG: track hit
            #if UNITY_EDITOR
            m_DebugHits.Add(hit);
            #endif

            // find the center of the capsule relative to the hit. it should be the intersection
            // of the capsule's axis and the cast direction
            var hitCapsuleCenter = (Vector3)default;

            // first find the capsule's axis: the normal from any collision point always points
            // towards the capsule's axis at a distance of the radius.
            //     ___
            //  .'  ‖  '.
            // ❘    C  <-❘
            // |'.  ‖  .'|
            // |   ‾‖‾   |
            // |->  C    |
            var axisPoint = hit.point + hit.normal * cast.Radius;

            // check the cast's colinearity with the capsule's axis
            var castDotUp = Vector3.Dot(cast.Direction, capsule.Up);

            // if they're colinear, we can't intersect them
            if (Mathf.Abs(castDotUp) > 0.99f) {
                // but we know that the hit can only have been the center of one of the
                // capsule's caps, so instead subtract (h / 2 - r)
                hitCapsuleCenter = axisPoint - Mathf.Sign(castDotUp) * (capsule.Height * 0.5f - capsule.Radius) * capsule.Up;
            }
            // otherwise the center is the intersection of the cast ray and the capsule's
            // vertical axis (any center + up)
            else {
                // find the intersection between the cast ray and the capsule's axis. to deal
                // with float precision errors, we intersect the cast with a plane containing the axis
                // and that is orthogonal to the plane containing the axis and cast
                var axis = new Ray(axisPoint, capsule.Up);
                // var localCast = new Ray(movePos, cast.Direction);

                // try to intersect the ray and the plane
                if (cast.IntoRay().TryIntersectIncidencePlane(axis, out var intersection)) {
                    hitCapsuleCenter = intersection;
                }
                // this should not happen; but if it does abort the collision from the last
                // successful cast
                else {
                    Debug.LogError($"[controller] HUGE MISTAKE, THIS SHOULD NEVER HAPPEN EVER EVER..., {Mathf.Abs(castDotUp)}");
                    break;
                }
            }

            // update move state; next move starts from capsule center and remaining distance
            moveEnd = hitCapsuleCenter;

            // calculate the remaining movement
            // TODO: should have something to do with the angle, considering wall hits
            var overshoot = moveTarget - moveEnd;
            moveDelta = Vector3.ProjectOnPlane(overshoot.normalized, hit.normal) * overshoot.magnitude;
            var groundAngle = Vector3.Angle(overshoot, moveDelta);

            // limit the move delta when hitting walls or over some custom slope function
            if(CustomSlopeAngleLossFunction != null) {
                moveDelta *= CustomSlopeAngleLossFunction(groundAngle);
            } else {
                moveDelta *= LimitSlopeToWallAngle(groundAngle);
            }

            // apply the contact offset
            moveEnd += hit.normal * m_ContactOffset;

            // if we touch any ground surface, we're grounded
            var surface = CollisionSurface.Ground;

            if (Vector3.Angle(hit.normal, Vector3.up) <= m_WallAngle) {
                if (!isGrounded) {
                    isGrounded = true;
                }
            } else {
                surface = CollisionSurface.Wall;
            }

            // update hit normal each cast
            m_HitNormal = hit.normal;

            // add this collision to the list
            m_Collisions.Add(new CharacterCollision(hit.normal, hit.point, surface));

            // update state
            i++;
        }

        // grounded if any cast hit ground
        m_IsGrounded = isGrounded;

        // store movement; subtract total contact offset when calculating velocity
        m_Position = moveEnd;
        m_Velocity = (moveEnd - moveStart) / Time.deltaTime;
    }

    // -- queries --
    /// the stored final position of the movement
    public Vector3 Position {
        get => m_Position;
    }

    /// the stored final velocity of the movement
    public Vector3 Velocity {
        get => m_Velocity;
    }

    /// the angle considered a wall
    public float WallAngle {
        get => m_WallAngle;
    }

    /// if the character is touching the ground
    public bool IsGrounded {
        get => m_IsGrounded;
    }

    /// the collisions this frame
    public Buffer<CharacterCollision> Collisions {
        get => m_Collisions;
    }

    // -- gizmos --
    #if UNITY_EDITOR
    /// draw gizmos for the controller
    public void DrawGizmos() {
        // draw the cast lollipops
        foreach (var cast in m_DebugCasts) {
            var o1 = cast.Radius * Vector3.up;
            var o2 = cast.Direction * cast.Length;

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(cast.Point2, cast.Radius);
            Gizmos.DrawLine(cast.Point1 - o1, cast.Point2 + o1);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(cast.Point2 + o2, cast.Radius);
            Gizmos.DrawLine(cast.Point1 - o1 + o2, cast.Point2 + o1 + o2);
        }

        // draw spheres where the casts hit
        foreach (var hit in m_DebugHits) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(hit.point, 0.05f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(hit.point, hit.normal * 0.5f);
        }

        UnityEditor.Handles.color = Color.yellow;
        var right = Quaternion.AngleAxis(90, Vector3.up) * m_Velocity.normalized;
        UnityEditor.Handles.Label(
            m_Position - right * 1.5f,
            $"casts: {m_DebugCasts.Count} hits: {m_DebugHits.Count}"
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(m_Position, 0.1f);
    }
    #endif
}
}