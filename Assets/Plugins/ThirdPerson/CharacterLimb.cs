using UnityEngine;

namespace ThirdPerson {

/// an ik limb for the character model
public sealed class CharacterLimb: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the type of goal of this limb")]
    [SerializeField] AvatarIKGoal m_Goal;

    // -- tuning --
    [Header("tuning")]
    [Tooltip("the max distance before searching for a new dest")]
    [SerializeField] float m_MaxDistance;

    [Tooltip("the move speed of the ik position")]
    [SerializeField] float m_MoveSpeed;

    [Tooltip("the turn speed of the ik rotation")]
    [SerializeField] float m_TurnSpeed;

    [Tooltip("the duration of the ik blend when dropping target")]
    [SerializeField] float m_BlendDuration;

    // -- props --
    /// if the limb is moving towards something
    bool m_HasTarget;

    /// the blending weight for this limb
    float m_Weight;

    /// the current ik position of the limb
    Vector3 m_CurrPosition;

    /// the current ik rotation of the limb
    Quaternion m_CurrRotation;

    /// the destination ik position of the limb
    Vector3 m_DestPosition;

    /// the destination ik rotation of the limb
    Quaternion m_DestRotation;

    // -- lifecycle --
    void Update() {
        var delta = Time.deltaTime;

        // lerp the ik position towards destination
        m_CurrPosition = Vector3.MoveTowards(
            m_CurrPosition,
            transform.InverseTransformPoint(m_DestPosition),
            m_MoveSpeed * Time.deltaTime
        );

        // lerp the weight down when there's no target
        m_Weight = Mathf.MoveTowards(
            m_Weight,
            m_HasTarget ? 1.0f : 0.0f,
            delta / m_BlendDuration
        );
    }

    // -- queries --
    /// the key for the specific limb
    public AvatarIKGoal Goal {
        get => m_Goal;
    }

    /// if the ik is currently active for this limb
    public bool IsActive {
        get => gameObject.activeSelf && m_Weight > 0.0f;
    }

    /// the current ik blending weight
    public float Weight {
        get => m_Weight;
    }

    /// the current ik position
    public Vector3 Position {
        get => transform.TransformPoint(m_CurrPosition);
    }

    /// the current ik rotation
    public Quaternion Rotation {
        get => m_CurrRotation;
    }

    // -- events --
    void OnTriggerEnter(Collider other) {
        var pos = other.ClosestPoint(transform.position);
        var sqrDist = Vector3.SqrMagnitude(pos - m_DestPosition);
        if (!IsActive || sqrDist > m_MaxDistance * m_MaxDistance) {
            // set current position to the hand's default position
            m_CurrPosition = transform.localPosition;

            // move towards the closest point on sruface
            m_DestPosition = pos;
        }

        // activate ik
        m_HasTarget = true;
    }

    void OnTriggerStay(Collider other) {
        m_HasTarget = true;

        var pos = other.ClosestPoint(transform.position);
        var sqrDist = Vector3.SqrMagnitude(pos - m_DestPosition);
        if (sqrDist > m_MaxDistance * m_MaxDistance) {
            m_DestPosition = pos;
        }
    }

    void OnTriggerExit(Collider other) {
        m_HasTarget = false;
    }
}

}