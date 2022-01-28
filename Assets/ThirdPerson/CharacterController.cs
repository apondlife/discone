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
    Vector3 m_LastHitNormal = Vector3.up;

    /// the last collision hit
    List<RaycastHit> m_DebugHits = new List<RaycastHit>();

    /// the list of casts this frame
    List<Capsule.Cast> m_DebugCasts = new List<Capsule.Cast>();

    // -- commands --
    /// move the character by a position delta
    public void Move(Vector3 delta) {
        // if the move was big enough to fire
        if (delta.magnitude <= m_MinMove) {
            return;
        }

        var t = m_Transform;
        var c = m_Capsule;

        // calculate capsule
        var capsule = Capsule.From(
            c.center,
            c.radius,
            c.height,
            t.up
        );

        // asdfasdfasdfasdfadsf TODO
        var p0 = t.position;
        var p1 = p0;
        var hit = (RaycastHit)default;

        m_DebugCasts.Clear();
        m_DebugHits.Clear();
        var i = 0;

        while (true) {
            // TODO: is this right?
            var mag = delta.magnitude;
            if (mag <= m_MinMove) {
                break;
            }

            if (i > 5) {
                Debug.LogError("OOPS");
                break;
            }

            var castDir = delta.normalized;
            var castLen = mag + m_CastOffset;
            var castPos = p1 - castDir * m_CastOffset + m_LastHitNormal * 0.01f;

            var cast = capsule.IntoCast(
                castPos,
                castDir,
                castLen
            );

            m_DebugCasts.Add(cast);

            // check for a collision
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

            // if we missed, move in the desired direction
            var pd = p1 + delta;
            if (!didHit) {
                p1 = pd;
                break;
            }

            m_DebugHits.Add(hit);

            // otherwise, calculate a new delta (TODO: more comment)
            var ch2 = c.height * 0.5f;
            var p1n = hit.point + ch2 * Vector3.up;
            delta = Vector3.ProjectOnPlane(pd - p1n, hit.normal);

            p1 = p1n;
            m_LastHitNormal = hit.normal;

            i++;
        }

        // set grounded if colliding w/ ground
        m_IsGrounded = Vector3.Angle(hit.normal, Vector3.up) <= m_MaxGroundAngle;

        // update state
        t.position = p1;
        m_Velocity = (p1 - p0) / Time.deltaTime;
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