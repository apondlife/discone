using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {

// TODO: center of mass? move character down?
/// an ik limb for the character model
public partial class Limb: CharacterBehaviour, CharacterPart, LimbContainer {
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
    [FormerlySerializedAs("m_StrideSystem")]
    [SerializeField] StrideSystem m_System;

    // -- props --
    /// the limb state
    LimbState m_State;

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
    public override void Init(CharacterContainer c) {
        base.Init(c);

        // set props
        m_State = new LimbState();

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
        m_System.Init(this);
    }

    public override void Step_Fixed_I(float delta) {
        if (!IsValid) {
            return;
        }

        m_System.Update(delta);

        // set target ik weight if limb is active
        var weight = m_State.IsActive ? 1f : 0f;

        // interpolate the weight
        var blendSpeed = weight > m_Weight ? m_Tuning.Blend_InSpeed : m_Tuning.Blend_OutSpeed;
        weight = Mathf.MoveTowards(
            m_Weight,
            weight,
            blendSpeed * delta
        );

        // the current placement normal
        var placement = m_State.Placement;
        var normal = placement.Result switch {
            LimbPlacement.CastResult.Hit =>
                placement.Normal,
            LimbPlacement.CastResult.OutOfRange when m_State.IsHeld && m_State.HeldDistance <= m_Tuning.HeldDistance_OnSurface =>
                placement.Normal,
            _ => Vector3.zero
        };

        // get next position goal position, removing the end offset if necessary
        var goalPos = m_State.GoalPos;
        if (normal != Vector3.zero) {
            goalPos += m_EndLen * normal;
        }

        // if not held, interpolate position.
        if (!m_State.IsHeld) {
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
        Debug_Step_Fixed();
    }

    #if UNITY_EDITOR
    void OnValidate() {
        var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject);
        if (!prefabStage) {
            return;
        }

        var prefabRoot = prefabStage.prefabContentsRoot;
        if (!prefabRoot || !prefabRoot.name.Contains("Character")) {
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
    /// set if the limb is striding
    public void SetIsStriding(bool isStriding) {
        if (m_State.IsNotStriding != isStriding) {
            return;
        }

        if (!isStriding) {
            m_System.ChangeTo(StrideSystem.NotStriding);
        } else {
            m_System.ChangeTo(StrideSystem.Free);
        }
    }

    /// switch to the moving state
    public void Move() {
        m_System.ChangeTo(StrideSystem.Moving);
    }

    /// switch to the holding state
    public void Hold(float delta) {
        if (!m_State.IsHeld) {
            m_System.ChangeToImmediate(StrideSystem.Holding, delta);
        }
    }

    /// release the limb if it's not already
    public void Release() {
        if (!m_State.IsFree) {
            m_System.ChangeTo(StrideSystem.Free);
        }
    }

    /// set the slide offset to translate the legs
    public void SetSlideOffset(Vector3 offset) {
        m_State.SlideOffset = offset;
    }

    // -- CharacterPart --
    public void ApplyIk() {
        if (!IsValid) {
            return;
        }

        var anim = c.Rig.Animator;

        anim.SetIKPositionWeight(
            m_Goal,
            m_Weight
        );

        anim.SetIKRotationWeight(
            m_Goal,
            m_Weight
        );

        if (m_Weight != 0.0f) {
            anim.SetIKPosition(
                m_Goal,
                m_GoalPos
            );

            anim.SetIKRotation(
                m_Goal,
                m_GoalRot
            );
        }

        Debug_ApplyIk();
    }

    public bool MatchesStep(CharacterEvent mask) {
        return (mask & Goal.AsStepEvent()) != 0;
    }

    public LimbPlacement Placement {
        get => State.Placement;
    }

    // -- queries --
    /// if this limb has the dependencies it needs to apply ik
    bool IsValid {
        get => m_GoalBone;
    }

    /// the current goal bone position
    public Vector3 GoalPos {
        get => m_State.GoalPos;
    }

    /// create an anchor from the current state of this limb
    public LimbAnchor IntoAnchor() {
        return new LimbAnchor(
            rootPos: transform.position,
            goalPos: m_State.GoalPos
        );
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

    /// the state for the limb
    public LimbState State {
        get => m_State;
    }

    /// the character container
    public CharacterContainer Character {
        get => c;
    }

    /// the length of the limb
    public float InitialLen {
        get => m_LimbLen + m_EndLen;
    }

    /// the direction towards the surface
    public Vector3 SearchDir {
        // TODO: make sure SearchDir is always normalized
        // TODO: we just want to remove tilt from the limb, could probably invert it somewhere
        get => Quaternion.LookRotation(c.State.Curr.Forward, Vector3.up) * m_SearchDir;
    }
}

}