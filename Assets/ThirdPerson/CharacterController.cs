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

    /// the last collision ray cast
    List<(Vector3 pos, Vector3 dir, float rad)> m_DebugRay = new List<(Vector3, Vector3, float)>();

    /// the last collision hit
    List<Vector3> m_DebugHit = new List<Vector3>();

    // -- commands --
    /// move the character by a position delta
    public void Move(Vector3 delta) {
        // if the move was big enough to fire
        if (delta.magnitude <= m_MinMove) {
            return;
        }

        var t = m_Transform;

        // calculate capsule
        // TODO: pre-calculate? create a struct?
        var c = m_Capsule;
        var cd = (c.height * 0.5f - c.radius) * Vector3.up;
        var capsulePt1 = c.center - cd;
        var capsulePt2 = c.center + cd;

        // asdf
        var p0 = t.position;
        var p1 = p0;

        var hit = (RaycastHit)default;

        var i = 0;
        m_DebugRay.Clear();
        m_DebugHit.Clear();
        while (true) {
            var mag = delta.magnitude;
            if (mag <= m_MinMove) {
                break;
            }

            if (i > 5) {
                Debug.LogError("OOPS");
                break;
            }

            var rayDir = delta.normalized;
            var rayLen = mag + m_CastOffset;

            var rayPos = p1 - rayDir * m_CastOffset + m_LastHitNormal * 0.01f;
            var rayPt1 = rayPos + capsulePt1;
            var rayPt2 = rayPos + capsulePt2;

            // check for a collision
            var didHit = Physics.CapsuleCast(
                rayPt1,
                rayPt2,
                c.radius,
                rayDir,
                out hit,
                rayLen,
                m_CollisionMask,
                QueryTriggerInteraction.Ignore
            );

            // if we missed, move in the desired direction
            var pd = p1 + delta;
            if (!didHit) {
                p1 = pd;
                break;
            }

            m_DebugRay.Add((rayPt1, rayDir * rayLen, c.radius));
            m_DebugHit.Add(hit.point);

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

        // update debug state
        // m_DebugHit = hit.point;
        // m_DebugRay = ray;

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
        foreach (var ray in m_DebugRay) {
            var p0 = ray.pos;
            var p1 = p0 + ray.dir;

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(p0, ray.rad);

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(p0, p1);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(p1, ray.rad);
        }

        foreach (var hit in m_DebugHit) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(hit, 0.05f);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(m_Transform.position, 0.1f);
    }
}
}