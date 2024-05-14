using UnityEngine;

namespace ThirdPerson {

// TODO: center of mass? move character down?
/// an ik limb for the character model
public partial class Limb: MonoBehaviour, CharacterPart, LimbAnchor, LimbContainer {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the type of goal of this limb")]
    [SerializeField] AvatarIKGoal m_Goal;

    [Tooltip("the tuning")]
    [SerializeField] LimbTuning m_Tuning;

    [Tooltip("the goal bone")]
    [SerializeField] Transform m_GoalBone;

    [Tooltip("the end bone")]
    [SerializeField] Transform m_EndBone;

    [HideInInspector]
    [Tooltip("the initial position of the goal bone")]
    [SerializeField] Vector3 m_InitialGoalPos;

    [HideInInspector]
    [Tooltip("the initial position of the end bone")]
    [SerializeField] Vector3 m_InitialEndPos;

    [Tooltip("the direction this limb searches for a surface")]
    [SerializeField] Vector3 m_SearchDir;

    // -- systems --
    [Header("systems")]
    [Tooltip("the limb system")]
    [SerializeField] StrideSystem m_StrideSystem;

    // -- props --
    /// the containing character
    CharacterContainer c;

    /// the animator
    Animator m_Animator;

    /// the current blend weight
    float m_Weight;

    /// the initial distance to the goal
    float m_LimbLen;

    /// the offset of the end bone used for placement, if any
    float m_EndLen;

    /// the position of the ik goal
    Vector3 m_GoalPos;

    /// the rotation of the ik goal
    Quaternion m_GoalRot;

    // -- lifecycle --
    void Update() {
        if (!IsValid) {
            return;
        }

        var delta = Time.deltaTime;
        m_StrideSystem.Update(delta);

        // set target ik weight if limb is active
        var weight = m_StrideSystem.IsActive ? 1f : 0f;

        // interpolate the weight
        var blendSpeed = weight > m_Weight ? m_Tuning.Blend_InSpeed : m_Tuning.Blend_OutSpeed;
        weight = Mathf.MoveTowards(
            m_Weight,
            weight,
            blendSpeed * delta
        );

        var normal = m_StrideSystem.Normal;

        // get next position goal position, removing the end offset if necessary
        var goalPos = m_StrideSystem.GoalPos;
        if (normal != Vector3.zero) {
            goalPos += m_EndLen * normal;
        }

        // if not held, interpolate position.
        if (!m_StrideSystem.IsHeld) {
            var goalDist = Vector3.SqrMagnitude(goalPos - m_GoalPos);
            goalPos = Vector3.MoveTowards(
                m_GoalPos,
                goalPos,
                m_Tuning.Goal_MoveSpeed * goalDist * delta
            );
        }

        // get next rotation
        var up = normal;

        // if no normal, use the direction towards the root
        if (up == Vector3.zero) {
            up = Vector3.Normalize(transform.position - goalPos);
        }

        var goalRot = Quaternion.LookRotation(
            Vector3.ProjectOnPlane(c.State.Curr.Forward, up),
            up
        );

        // always interpolate rotation
        var goalRotDist = Quaternion.Angle(goalRot, m_GoalRot);
        goalRot = Quaternion.RotateTowards(
            m_GoalRot,
            goalRot,
            m_Tuning.Goal_RotationSpeed * goalRotDist * delta
        );

        // update state
        m_Weight = weight;
        m_GoalPos = goalPos;
        m_GoalRot = goalRot;

        // TODO: consider how to compile out debug utils; DEVELOPMENT_BUILD, DEBUG?
        Debug_Update();
    }

    #if UNITY_EDITOR
    void OnValidate() {
        var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject);
        var character = prefabStage.prefabContentsRoot.GetComponent<Character>();
        if (!character) {
            return;
        }

        // disallow rotation, search dir is local
        transform.rotation = Quaternion.identity;

        // cache positions of the bones
        m_InitialGoalPos = FindInitialBonePos(m_GoalBone);
        m_InitialEndPos = FindInitialBonePos(m_EndBone);
    }

    Vector3 FindInitialBonePos(Transform bone) {
        return bone ? transform.InverseTransformPoint(bone.transform.position) : Vector3.zero;
    }
    #endif

    // -- commands --
    /// initialize this limb w/ an animator
    public void Init(Animator animator) {
        // set deps
        c = GetComponentInParent<CharacterContainer>();

        // set props
        m_Animator = animator;

        // get default limb lengths
        var rootPos = Vector3.zero;
        var goalPos = m_InitialGoalPos;
        var endPos = m_InitialEndPos;

        var limb = goalPos - rootPos;
        var limbDir = limb.normalized;

        var end = endPos - goalPos;
        var endLength = Vector3.Dot(end, limbDir);

        m_LimbLen = limb.magnitude;
        m_EndLen = endLength;

        // init system
        m_StrideSystem.Init(this);
    }

    /// toggles stride system enabled
    public void SetIsStriding(bool isStriding) {
        m_StrideSystem.SetIsStriding(isStriding);
    }

    /// starts a new stride for the limb
    public void Move(LimbAnchor anchor) {
        m_StrideSystem.Move(anchor);
    }

    /// holds the limb if not already
    public void Hold(float delta) {
        m_StrideSystem.Hold(delta);
    }

    /// released the limb's hold if not already
    public void Release() {
        m_StrideSystem.Release();
    }

    /// set the slide offset to translate the legs
    public void SetSlideOffset(Vector3 offset) {
        m_StrideSystem.SetSlideOffset(offset);
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

        m_Animator.SetIKRotationWeight(
            m_Goal,
            m_Weight
        );

        if (m_Weight != 0.0f) {
            m_Animator.SetIKPosition(
                m_Goal,
                m_GoalPos
            );

            m_Animator.SetIKRotation(
                m_Goal,
                m_GoalRot
            );
        }

        Debug_ApplyIk();
    }

    // -- queries --
    /// if this limb has the dependencies it needs to apply ik
    bool IsValid {
        get => m_GoalBone;
    }

    /// the current goal bone position
    public Vector3 GoalPos {
        get => m_StrideSystem.GoalPos;
    }

    /// .
    public bool IsFree {
        get => m_StrideSystem.IsFree;
    }

    /// .
    public bool IsHeld {
        get => m_StrideSystem.IsHeld;
    }

    /// cast for the distance to the nearest surface in the limb direction
    public float HeldDistance {
        get => m_StrideSystem.HeldDistance;
    }

    // -- CharacterLimbContainer --
    /// the ik goal
    public AvatarIKGoal Goal {
        get => m_Goal;
    }

    /// the current root bone position
    public Vector3 RootPos {
        get => transform.position;
    }

    /// the tuning for the limb
    public LimbTuning Tuning {
        get => m_Tuning;
    }

    /// the character container
    public CharacterContainer Character {
        get => c;
    }

    /// the bone the stride is anchored by
    public LimbAnchor InitialAnchor {
        get => this;
    }

    /// the length of the limb
    public float InitialLen {
        get => m_LimbLen + m_EndLen;
    }

    /// the direction towards the surface
    public Vector3 SearchDir {
        get => transform.TransformDirection(m_SearchDir);
    }
}

}