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

    // -- props --
    /// the character's current velocity
    Vector3 m_Velocity;

    /// if the character is touching the ground
    bool m_IsGrounded;

    // -- commands --
    /// move the character by a position delta
    public void Move(Vector3 delta) {
        // if the move was big enough to fire
        var mag = delta.magnitude;
        if (mag <= m_MinMove) {
            return;
        }

        var t = m_Transform;

        // calculate new position
        var p0 = t.position;

        var dir = delta.normalized;
        var rayOrigin = p0 - dir * m_CastOffset;
        var len = mag + m_CastOffset;
        var moveRay = new Ray(rayOrigin, dir * len);
        var p1 = p0 + delta;

        if (Physics.Raycast(moveRay, out var hit, len, m_CollisionMask)) {
            var pc = hit.point;
            p1 -= Vector3.Project(p1 - pc, hit.normal);

            // set grounded if colliding w/ ground
            var angle = Vector3.Angle(hit.normal, Vector3.up);
            m_IsGrounded = angle <= m_MaxGroundAngle;

            lastHit = pc;
        }

        debugRay = moveRay;

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

    private Ray debugRay;
    private Vector3 lastHit;
    public void DrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(debugRay);
        Gizmos.DrawSphere(m_Transform.position, 0.2f);
        Gizmos.DrawSphere(lastHit, 0.4f);
    }
}
}