using UnityEngine;

namespace ThirdPerson {

// a position below the target, on the ground if possible
public class CameraLookAtTarget: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the max distance from the character to cast for the ground")]
    [SerializeField] private float m_MaxDistance;

    [UnityEngine.Serialization.FormerlySerializedAs("m_TargetSpeed")]
    [SerializeField] private float m_MaxSpeed;

    [SerializeField] private float m_TargetAcceleration;

    [Tooltip("the max distance from the character to cast for the ground")]
    [SerializeField] private float m_MinFallingSpeed;
    [SerializeField] private float m_MaxFallingSpeed;

    [Tooltip("the layer mask for the ground")]
    [SerializeField] private LayerMask m_GroundLayers;

    // -- refs --
    [Header("refs")]
    [Tooltip("the target transform")]
    [SerializeField] private Transform m_Target;

    // -- props --
    /// a reference to the character state
    CharacterState m_State;

    float m_TargetSpeed;

    // -- lifecycle --
    void Start() {
        // set deps
        var character = GetComponentInParent<Character>();
        m_State = character.State;
    }

    void Update() {
        var pos = transform.localPosition;
        var target = m_Target.localPosition;

        // if falling
        if (m_State.IsGrounded) {
            target = pos;
        }
        // move target to ground position underneath character
        else {
            // snap to the lowest possible position
            var delta = Vector3.down * m_MaxDistance;
            // move to ground if there's a ground layer closer
            if (Physics.Raycast(pos, Vector3.down, out var hit, m_MaxDistance, m_GroundLayers)) {
                delta = hit.point - transform.position;
            }

            // lerp based on fall speed
            var fallParam = Mathf.InverseLerp(m_MinFallingSpeed, m_MaxFallingSpeed, -m_State.Velocity.y);
            delta *= fallParam;

            target = transform.localPosition + delta;
        }

        if (m_Target.position == target) {
            m_TargetSpeed = 0.0f;
        } else {
            m_TargetSpeed = Mathf.MoveTowards(m_TargetSpeed, m_MaxSpeed, m_TargetAcceleration * Time.deltaTime);
        }

        m_Target.localPosition = Vector3.MoveTowards(m_Target.localPosition, target, m_TargetSpeed * Time.deltaTime);
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