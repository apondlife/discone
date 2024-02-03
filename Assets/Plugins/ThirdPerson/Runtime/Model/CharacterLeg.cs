using UnityEngine;

namespace ThirdPerson {

/// an ik limb for the character model
public sealed class CharacterLeg: MonoBehaviour, CharacterLimb {
    // -- deps --
    /// the containing character
    Character m_Container;

    /// the animator for this limb
    Animator m_Animator;

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the type of goal of this limb")]
    [SerializeField] AvatarIKGoal m_Goal;

    [Tooltip("the anchor transform for collision checks")]
    [SerializeField] Transform m_Anchor;

    // -- tuning --
    [Header("tuning")]
    [Tooltip("the move speed of the ik position")]
    [SerializeField] float m_MoveSpeed;

    [Tooltip("the turn speed of the ik rotation")]
    [SerializeField] float m_TurnSpeed;

    [Tooltip("the duration of the ik blend when dropping target")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_BlendDuration")]
    [SerializeField] float m_BlendInDuration;

    [Tooltip("the duration of the ik blend when dropping target")]
    [SerializeField] float m_BlendOutDuration;

    [UnityEngine.Serialization.FormerlySerializedAs("m_MaxDistance")]
    [Tooltip("the max distance before searching for a new dest")]
    [SerializeField] float m_StrideLength;

    // -- props --
    /// if the limb is moving towards something
    bool m_IsActive;

    /// if the limb is moving towards something
    bool m_HasTarget;

    /// the transform of the goal bone, if any
    Transform m_AnimatedBone;

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

    /// the square stride length
    float m_SqrStrideLength;

    // -- lifecycle --
    void Awake() {
        // set deps
        m_Container = GetComponentInParent<Character>();

        // cache stride length
        m_SqrStrideLength = m_StrideLength * m_StrideLength;
    }

    void FixedUpdate() {
        // feets are active when we're airborne
        SetIsActive(!m_Container.State.Next.IsOnGround);
    }

    void Update() {
        if (!IsValid) {
            return;
        }

        var delta = Time.deltaTime;

        // lerp the weight
        var isBlendingIn = m_IsActive && m_HasTarget;
        m_Weight = Mathf.MoveTowards(
            m_Weight,
            isBlendingIn ? 1.0f : 0.0f,
            delta / (isBlendingIn ? m_BlendInDuration : m_BlendOutDuration)
        );

        // lerp the ik position towards destination
        if (m_IsActive) {
            m_CurrPosition = Vector3.MoveTowards(
                m_CurrPosition,
                transform.InverseTransformPoint(m_DestPosition),
                m_MoveSpeed * Time.deltaTime
            );
        }
    }

    // -- commands --
    /// initialize this limb w/ an animator
    public void Init(Animator animator) {
        // set props
        m_Animator = animator;

        // cache the bone; we can't really do anything if we don't find a bone
        m_AnimatedBone = m_Animator.GetBoneTransform(m_Goal switch {
            AvatarIKGoal.RightFoot => HumanBodyBones.RightFoot,
            _ /*AvatarIKGoal.LeftFoot*/ => HumanBodyBones.LeftFoot,
        });

        // error on misconfiguration
        if (!IsValid) {
            Debug.LogError($"[chrctr] {m_Container.name} - has a limb w/ no matching bone: {Goal}");
        }
    }

    /// update if ik is active for is this lime
    public void SetIsActive(bool isActive) {
        if (!isActive) {
            m_HasTarget = false;
        }

        m_IsActive = isActive;
    }

    /// applies the limb ik
    public void ApplyIk() {
        if (!IsValid) {
            return;
        }

        m_Animator.SetIKPositionWeight(
            m_Goal,
            m_Weight
        );

        if (m_Weight != 0.0f) {
            m_Animator.SetIKPosition(
                m_Goal,
                transform.TransformPoint(m_CurrPosition)
            );
        }
    }

    // -- queries --
    /// if this limb has the dependencies it needs to apply ik
    public bool IsValid {
        get => m_AnimatedBone != null;
    }

    /// .
    public AvatarIKGoal Goal {
        get => m_Goal;
    }

    /// if moving to this position completes a stride
    bool HasCompletedStrideAt(Vector3 pos) {
        return Vector3.SqrMagnitude(pos - m_DestPosition) >= m_SqrStrideLength;
    }

    // -- events --
    void OnTriggerEnter(Collider other) {
        if (!m_IsActive || !IsValid) {
            return;
        }

        // ignore terrain since getting closest point for terrains will be very unlikely
        if (other is TerrainCollider) {
            return;
        }

        // can't use closest point on concave meshes
        // TODO: consider trying a raycast here
        if (other is MeshCollider m && !m.convex) {
            return;
        }

        var pos = other.ClosestPoint(m_Anchor.position);
        if (!m_HasTarget || HasCompletedStrideAt(pos)) {
            // start tracking the target
            m_HasTarget = true;

            // set current position from the bone's current position in our local space
            m_CurrPosition = transform.InverseTransformPoint(m_AnimatedBone.position);

            // move towards the closest point on sruface
            m_DestPosition = pos;
        }
    }

    void OnTriggerStay(Collider other) {
        if (!m_IsActive || !IsValid) {
            return;
        }

        // can't use closest point on concave meshes
        // TODO: consider trying a raycast here
        if (other is MeshCollider m && !m.convex) {
            return;
        }

        m_HasTarget = true;

        var pos = other.ClosestPoint(m_Anchor.position);
        if (HasCompletedStrideAt(pos)) {
            m_DestPosition = pos;
        }
    }

    void OnTriggerExit(Collider other) {
        if (!m_IsActive || !IsValid) {
            return;
        }

        m_HasTarget = false;
    }

    // -- gizmos --
    void OnDrawGizmos() {
        if (!m_IsActive || !m_HasTarget) {
            return;
        }

        var currPos = transform.TransformPoint(m_CurrPosition);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(
            currPos,
            radius: 0.05f
        );

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(
            m_DestPosition,
            radius: 0.15f
        );

        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            currPos,
            m_DestPosition
        );
    }
}

}