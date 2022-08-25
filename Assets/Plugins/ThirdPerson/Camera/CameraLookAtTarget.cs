using UnityEngine;

namespace ThirdPerson {

// a position below the target, on the ground if possible
public class CameraLookAtTarget: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the max distance from the character to cast for the ground")]
    [SerializeField] private float m_MaxDistance;

    [Header("Ground Target")]
    [SerializeField] private float m_TargetSpringDown;

    [UnityEngine.Serialization.FormerlySerializedAs("m_TargetDamp")]
    [SerializeField] private float m_TargetSpeed;

    [UnityEngine.Serialization.FormerlySerializedAs("m_TargetDamp")]
    [SerializeField] private float m_TargetDampDown;

    [SerializeField] private float m_TargetSpringUp;
    [SerializeField] private float m_TargetDampUp;

    [Tooltip("the max distance from the character to cast for the ground")]
    [SerializeField] private float m_MinFallingSpeed;
    [SerializeField] private float m_MaxFallingSpeed;

    [Tooltip("the layer mask for the ground")]
    [SerializeField] private LayerMask m_GroundLayers;

    // -- vertical offset --
    [Header("Vertical Offset")]
    [Tooltip("the offset scale between 0 and min distance")]
    [SerializeField] private AnimationCurve m_VerticalOffset_DistanceCurve;

    [Tooltip("the maximum height to move the target up")]
    [SerializeField] float m_VerticalOffset_MaxHeight;

    // -- refs --
    [Header("refs")]
    [Tooltip("the target transform")]
    [SerializeField] private Transform m_Target;

    [Tooltip("the follow target")]
    [SerializeField] CameraFollowTarget m_Follow;

    [Tooltip("the character model")]
    [SerializeField] Transform m_Model;

    // -- props --
    /// a reference to the character state
    Character m_Character;

    float m_GroundTargetSpeed;

    // the stored position of where we want to look at towards the ground
    Vector3 m_GroundTarget;

    // -- lifecycle --
    void Start() {
        // set deps
        m_Character = GetComponentInParent<Character>();
        m_GroundTarget = transform.localPosition;
    }

    void Update() {
        Vector3 lookOffset = Vector3.zero;

        // first we find the ground target destination
        Vector3 groundDest = FindGroundDestination();

        // if we're at our ground target
        if (m_GroundTarget == groundDest) {
            // we may be grounded and want to look up when closej
            if (m_Character.State.IsGrounded) {
                // check proximity between model & follow target to push look at up
                var followDist = Vector3.Distance(
                    m_Model.position,
                    m_Follow.Position
                );

                var proximity = m_VerticalOffset_DistanceCurve.Evaluate(
                    Mathf.InverseLerp(
                        m_Follow.MinDistance,
                        m_Follow.BaseDistance,
                        followDist
                    )
                );

                lookOffset = m_VerticalOffset_MaxHeight * proximity * Vector3.up;
            }

            m_Target.localPosition = groundDest + lookOffset;
        }
        // TODO: add a comment here
        else {
            var dist = (m_GroundTarget - groundDest);
            var up = dist.y < 0;
            var spring = up ? m_TargetSpringUp : m_TargetSpringDown;
            var damp = up ? m_TargetDampUp : m_TargetDampDown;
            var acceleration = spring * dist.y - damp * m_GroundTargetSpeed;
            m_GroundTargetSpeed += Time.deltaTime * acceleration;

            // move the target
            m_GroundTarget = Vector3.MoveTowards(
                m_GroundTarget,
                groundDest,
                Mathf.Abs(m_GroundTargetSpeed * Time.deltaTime)
            );

            m_Target.localPosition = m_GroundTarget;
        }
    }

    /// find destination point for the ground target
    Vector3 FindGroundDestination() {
        // by default the ground destination is this transform's local pos,
        // which is at the model's feet
        var footPos = transform.localPosition;

        // if grounded, just use that
        if (m_Character.IsPaused || m_Character.State.IsGrounded) {
            return footPos;
        }

        // if airborne, move ground destination underneath character, snap to
        // the lowest possible position
        var delta = Vector3.down * m_MaxDistance;

        // if there's a closer ground layer, move to that point
        var didHit = Physics.Raycast(
            footPos,
            Vector3.down,
            out var hit,
            m_MaxDistance,
            m_GroundLayers
        );

        if (didHit) {
            delta = hit.point - transform.position;
        }

        // scale based on fall speed
        delta *= Mathf.InverseLerp(
            m_MinFallingSpeed,
            m_MaxFallingSpeed,
            -m_Character.State.Velocity.y
        );

        return footPos + delta;
    }

    // -- queries --
    /// how close the look at target is to full extension
    public float PercentExtended {
        get => Mathf.Clamp01(Vector3.Distance(transform.position, m_Target.position) / m_MaxDistance);
    }

    // -- debug --
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.down * m_MaxDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, m_Target.position);
        Gizmos.DrawSphere(m_Target.position, 0.3f);
    }
}

}