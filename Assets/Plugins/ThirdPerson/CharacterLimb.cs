using UnityEngine;

namespace ThirdPerson {

public sealed class CharacterLimb : MonoBehaviour
{
    [Header("the type of goal of this limb")]
    [SerializeField] AvatarIKGoal m_Goal;

    [Header("the target for ik")]
    [SerializeField] Transform m_Target;

    [Header("the target for ik")]
    [SerializeField] float m_MaxDistance;

    // if this limb is currently pointing to its target
    bool m_IsActive = false;

    // the world position this limb is targetting
    Vector3 m_Position;

    // -- queries --
    public bool IsActive {
        get => gameObject.activeSelf && m_IsActive;
    }

    public AvatarIKGoal Goal {
        get => m_Goal;
    }

    public Vector3 Position {
        get => m_Position;
    }

    public Quaternion Rotation {
        get => m_Target.rotation;
    }

    // -- events --
    void OnTriggerEnter(Collider other) {
        var pos = other.ClosestPoint(transform.position);
        if (!m_IsActive || Vector3.SqrMagnitude(pos - m_Target.position) > m_MaxDistance * m_MaxDistance) {
            m_Target.position = pos;
        }
        m_IsActive = true;
        Debug.Log($"{this.name} hit {other.name}");
    }

    void OnTriggerStay(Collider other) {
        m_IsActive = true;
        var pos = other.ClosestPoint(transform.position);
        var sqrDist = Vector3.SqrMagnitude(pos - m_Position);
        if (sqrDist > m_MaxDistance * m_MaxDistance) {
            m_Position = pos;
        }
        Debug.Log($"{this.name} stay {other.name} dist {sqrDist}");
    }

    void OnTriggerExit(Collider other) {
        m_IsActive = false;
    }
}
}