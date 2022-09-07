using UnityEngine;

namespace ThirdPerson {

// a position below the target, on the ground if possible
public class CameraLookAtTarget: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the max distance from the character to cast for the ground")]
    [SerializeField] float m_MaxDistance;

    [Header("Ground Target")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_TargetDamp")]
    [SerializeField] float m_TargetSpeed;

    [Tooltip("the spring daming when moving down")]
    [SerializeField] SpringDamp m_SpringDamp_Down;

    [Tooltip("the spring damping when moving up")]
    [SerializeField] SpringDamp m_SpringDamp_Up;

    [Tooltip("the spring damping when free look is on")]
    [SerializeField] SpringDamp m_SpringDamp_FreeLook;

    [Tooltip("the minimum target fall speed")]
    [SerializeField] float m_MinFallingSpeed;

    [Tooltip("the minimum target fall speed")]
    [SerializeField] float m_MaxFallingSpeed;

    [Tooltip("the layer mask for the ground")]
    [SerializeField] LayerMask m_GroundLayers;

    // -- vertical offset --
    [Header("vertical offset")]
    [Tooltip("the offset scale between 0 and min distance")]
    [SerializeField] AnimationCurve m_VerticalOffset_DistanceCurve;

    [Tooltip("the maximum height to move the target up")]
    [SerializeField] float m_VerticalOffset_MaxHeight;

    // -- refs --
    [Header("refs")]
    [Tooltip("the target transform")]
    [SerializeField] private Transform m_Target;

    [Tooltip("the follow target")]
    [SerializeField] CameraFollowTarget m_Follow;

    // -- props --
    /// a reference to the character
    Character m_Character;

    /// the stored position of where we want to look at towards the ground
    Vector3 m_GroundTarget;

    /// the current target speed
    float m_GroundTargetSpeed;

    // -- lifecycle --
    void Start() {
        // set deps
        m_Character = GetComponentInParent<Character>();
        m_GroundTarget = transform.localPosition;
    }

    void Update() {
        // first we find the ground target destination
        var groundDest = FindGroundDestination();
        var lookOffset = Vector3.zero;
        var freelook = m_Follow.IsFreeLookEnabled;

        // while airborne, move the ground target towards the ground
        if (m_GroundTarget != groundDest) {
            var dist = (m_GroundTarget - groundDest);
            var sd = (m_Follow.IsFreeLookEnabled, dist.y < 0.0f) switch {
                (true, _) => m_SpringDamp_FreeLook,
                (_, true) => m_SpringDamp_Up,
                (_, false) => m_SpringDamp_Down
            };

            var acceleration = sd.Spring * dist.y - sd.Damp * m_GroundTargetSpeed;
            m_GroundTargetSpeed += Time.deltaTime * acceleration;

            // move the target
            m_GroundTarget = Vector3.MoveTowards(
                m_GroundTarget,
                groundDest,
                Mathf.Abs(m_GroundTargetSpeed * Time.deltaTime)
            );
        }

        // we may be grounded and want to look up when close
        if (m_Follow.IsFreeLookEnabled) {
            // check proximity between model & follow target to push look at up
            var followDist = Vector3.Distance(
                m_Follow.transform.position, // TODO: should this be ground target?
                m_Follow.TargetPosition
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

        m_Target.localPosition = m_GroundTarget + lookOffset;
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

    [System.Serializable]
    struct SpringDamp {
        public float Spring;
        public float Damp;
    }
}

}