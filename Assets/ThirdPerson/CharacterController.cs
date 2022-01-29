using System.Collections.Generic;
using UnityEngine;

namespace ThirdPerson {

/// a re-implementation of unity's built-in character controller w/ better
/// collision handling
[System.Serializable]
sealed class CharacterController {
    // Why are we not using rigidbodies?
    // They seems to be more annoying to workaround,
    // since we are implementing collision from scratch, might as well implement it in a controlled way.
    // The best thing we would get from rigidbodies is the possibility of other objects colliding with the character, which we can implement as an add-on/child object

    // -- fields --
    [Header("config")]
    [Tooltip("the collision mask for the character")]
    [SerializeField] private LayerMask m_CollisionMask;

    [Tooltip("the minimum move vector to have any effect")]
    [SerializeField] private float m_MinMove;

    [Tooltip("the highest angle in which colliding with is considered ground. ie slope angle")]
    [SerializeField] private float m_MaxGroundAngle;

    [Tooltip("the amount to offset collision casts to avoid precision issues")]
    [SerializeField] float m_CastOffset;

    [Header("references")]
    [Tooltip("the character's transform")]
    [SerializeField] Transform m_Transform;

    [Tooltip("the character's capsule")]
    [SerializeField] CapsuleCollider m_Capsule;

    // -- props --
    /// the character's current velocity
    Vector3 m_Velocity;

    /// if the character is touching the ground
    bool m_IsGrounded;

    /// the normal of the last collision surface
    Vector3 m_HitNormal = Vector3.up;

    /// the last collision hit
    List<RaycastHit> m_DebugHits = new List<RaycastHit>();

    /// the list of casts this frame
    List<Capsule.Cast> m_DebugCasts = new List<Capsule.Cast>();

    // -- commands --
    /// move the character by a position delta
    public void Move(Vector3 delta) {
        // if the move was big enough to fire
        // TODO: is this necessary?
        if (delta.magnitude <= m_MinMove) {
            return;
        }

        // capture shorthand
        var t = m_Transform;

        // calculate capsule
        var c = m_Capsule;
        var capsule = new Capsule(
            c.center,
            c.radius,
            c.height,
            t.up
        );

        // track start and end position to calculate velocity
        var moveStart = t.position;
        var moveEnd = moveStart;
        var moveDelta = delta;

        // track hit surface state
        var hitNormal = m_HitNormal;
        var isGrounded = false;

        // DEBUG: reset state
        var i = 0;
        m_DebugCasts.Clear();
        m_DebugHits.Clear();

        // while there is any more to move
        while (true) {
            // TODO: is this necessary?
            var moveMag = moveDelta.magnitude;
            if (moveMag <= m_MinMove) {
                break;
            }

            // DEBUG: if we cast an unlikely number of times, stop
            if (i > 5) {
                Debug.LogError("cast more than 5 times in a single frame!");
                break;
            }

            // capsule cast the remaining move
            var castDir = moveDelta.normalized;
            var castLen = moveMag + m_CastOffset;
            var castPos = moveEnd - castDir * m_CastOffset + hitNormal * 0.01f;

            var cast = capsule.IntoCast(
                castPos,
                castDir,
                castLen
            );

            // DEBUG: track cast
            m_DebugCasts.Add(cast);

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
                break;
            }

            // DEBUG: track hit
            m_DebugHits.Add(hit);

            // update hit surface information
            hitNormal = hit.normal;

            // if we touch any ground surface, we're grounded
            if (!isGrounded && Vector3.Angle(hitNormal, Vector3.up) <= m_MaxGroundAngle) {
                isGrounded = true;
            }

            // find the center of the capsule relative to the hit
            // it should be the intersection of the capsules axis and the cast direction
            var hitCapsuleCenter = (Vector3)default;


            // first find the capsules axis:
            // from the collision point and normal, we get a point on the capsules axis.
            // the normal will always point to the axis, if its on the sphere, pointing to the spheres center
            var axisPoint = hit.point + hitNormal * cast.Radius;

            // if the cast is colinear with capsules axis, we cant intersect them
            var hitDotUp = Vector3.Dot(cast.Direction, capsule.Up);
            if (Mathf.Abs(hitDotUp) > 0.9999f) {
                // but we know that the hit can only have been on the sphere's surface,
                // for which the normal will always point towards the center,
                // meaning axisPoint is the center of the sphere.
                // therefore finding the capsules center is trivial
                hitCapsuleCenter = hit.point + Mathf.Sign(hitDotUp) * capsule.Height * 0.5f * capsule.Up;
            }
            // otherwise the center is the intersection of the cast ray and the capsule's
            // vertical axis (any center + up)
            else {
                // the hit is always radius away from the axis and its normal always points towards
                // the axis
                var axis = new Ray(axisPoint, capsule.Up);

                // find the intersection between the cast ray and axis
                if (cast.IntoRay().TryIntersect(axis, out var intersection)) {
                    hitCapsuleCenter = intersection;
                }
                // default to bottom of capsule
                else {
                    Debug.LogError("ray and center axis should interect");
                    hitCapsuleCenter = hit.point + capsule.Height * 0.5f * capsule.Up;
                }
            }

            // update move state; next move starts from capsule center and remaining distance
            moveEnd = hitCapsuleCenter;
            moveDelta = Vector3.ProjectOnPlane(moveTarget - moveEnd, hitNormal);

            // DEBUG: update state
            i++;
        }

        // update hit state
        m_HitNormal = hitNormal;
        m_IsGrounded = isGrounded;

        // move character
        t.position = moveEnd;
        m_Velocity = (moveEnd - moveStart) / Time.deltaTime;
    }

    // -- queries --
    /// the character's curent velocity
    public Vector3 velocity {
        get => m_Velocity;
    }

    /// if the character is touching the ground
    public bool isGrounded {
        get => m_IsGrounded;
    }

    // -- gizmos --
    public void DrawGizmos() {
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

        foreach (var hit in m_DebugHits) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(hit.point, 0.05f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(hit.point, hit.normal * 0.5f);
        }

        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.Label(
            m_Transform.position - m_Transform.right * 1.5f,
            $"casts: {m_DebugCasts.Count} hits: {m_DebugHits.Count}"
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(m_Transform.position, 0.1f);
    }
}
}