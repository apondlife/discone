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

    /// the last collision ray cast
    Ray m_DebugRay;

    /// the last collision hit
    Vector3 m_DebugHit;

    // -- commands --
    /// move the character by a position delta
    public void Move(Vector3 delta) {
        // if the move was big enough to fire
        var mag = delta.magnitude;
        if (mag <= m_MinMove) {
            return;
        }

        var t = m_Transform;

        // get position and projected next position
        var p0 = t.position;
        var p1 = p0 + delta;

        // build collision cast
        var rayDir = delta.normalized;
        var rayPos = p0 - rayDir * m_CastOffset;
        var rayLen = mag + m_CastOffset;
        var ray = new Ray(rayPos, rayDir * rayLen);

        // check for a collision
        var hit = (RaycastHit)default;

        var i = 0;
        while (true) {
            var didHit = Physics.Raycast(ray, out hit, rayLen, m_CollisionMask);
            if (!didHit) {
                break;
            }

            // calc vertical overshoot through the collision surface
            var overshoot = Vector3.Project(p1 - hit.point, hit.normal);
            if (overshoot == Vector3.zero) {
                break;
            }

            if (i > 0) {
                Debug.Log($"i {i} overshoot {overshoot} mag2 {overshoot} {overshoot == Vector3.zero}");
            }

            i++;

            // subtract overshoot from next position and store hit point
            p1 -= overshoot;
        }

        // set grounded if colliding w/ ground
        m_IsGrounded = Vector3.Angle(hit.normal, Vector3.up) <= m_MaxGroundAngle;

        // update debug state
        m_DebugHit = hit.point;
        m_DebugRay = ray;

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
        Gizmos.color = Color.green;
        Gizmos.DrawRay(m_DebugRay);
        Gizmos.DrawSphere(m_Transform.position, 0.2f);
        Gizmos.DrawSphere(m_DebugHit, 0.4f);
    }
}
}