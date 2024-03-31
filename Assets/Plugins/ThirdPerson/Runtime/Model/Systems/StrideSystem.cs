using System;
using Soil;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {

/// the character limb's stride tracking
[Serializable]
class StrideSystem: CharacterSystem {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the root bone")]
    [SerializeField] Transform m_Root;

    [Tooltip("the cast offset")]
    [SerializeField] float m_CastOffset;

    [FormerlySerializedAs("m_LayerMask")]
    [Tooltip("the cast layer mask")]
    [SerializeField] LayerMask m_CastMask;

    // -- tuning --
    [Header("tuning")]
    [Tooltip("the scale for the speed")]
    [SerializeField] MapInCurve m_SpeedScale;

    [Tooltip("the max distance before searching for a new dest")]
    [SerializeField] MapOutCurve m_StrideLength;

    // -- props --
    /// the ik goal
    AvatarIKGoal m_Goal;

    /// if the limb is currently free
    bool m_IsFree;

    /// if the limb is currently held
    bool m_IsHeld;

    /// the default limb length;
    float m_Length;

    /// the current ik position of the limb
    Vector3 m_GoalPos;

    /// the current ik rotation of the limb
    Quaternion m_GoalRot;

    /// the bone the stride is anchored by
    CharacterBone m_Anchor;

    /// an offset that translates the held position
    Vector3 m_Offset;

    // -- Soil.System --
    protected override Phase InitInitialPhase() {
        return Holding;
    }

    protected override SystemState State { get; set; } = new();

    // -- lifecycle --
    public void Init(
        CharacterContainer c,
        AvatarIKGoal goal,
        Vector3 goalPos,
        CharacterBone anchor
    ) {
        m_Goal = goal;
        m_GoalPos = goalPos;
        m_Anchor = anchor;
        m_Length = Vector3.Distance(anchor.RootPos, goalPos);

        base.Init(c);
    }

    // -- commands --
    /// switch to the moving state
    public void Move(CharacterBone anchor) {
        m_Anchor = anchor;
        ChangeTo(Moving);
    }

    /// release the limb if it's not already
    public void Release() {
        m_Anchor = null;
        if (!IsFree) {
            ChangeTo(Free);
        }
    }

    /// set the current offset to translate the legs
    public void SetOffset(Vector3 offset) {
        m_Offset = offset;
    }

    // -- Free --
    Phase Free => new(
        name: "Free",
        enter: Free_Enter,
        update: Free_Update,
        exit: Free_Exit
    );

    void Free_Enter() {
        m_IsFree = true;
        m_GoalPos = m_Root.position + RootDir * m_Length;
    }

    void Free_Update(float delta) {
        var didHit = FindSurface(m_GoalPos, out var hitPoint);
        if (didHit) {
            m_GoalPos = hitPoint;
            ChangeTo(Holding);
            return;
        }

        m_GoalPos = m_Root.position + RootDir * m_Length;
    }

    void Free_Exit() {
        m_IsFree = false;
    }

    // -- Moving --
    Phase Moving => new(
        name: "Moving",
        update: Moving_Update
    );

    void Moving_Update(float delta) {
        var v = c.State.Curr.SurfaceVelocity;
        var speedScale = m_SpeedScale.Evaluate(v.magnitude);
        var strideLength = m_StrideLength.Evaluate(speedScale);

        // the anchor leg vector
        var anchor = m_Anchor.RootPos - m_Anchor.GoalPos;
        var stride = Vector3.ProjectOnPlane(anchor, Vector3.up);

        // the direction to the goal; mirror the stride over anchor up
        var goalDir = stride;
        goalDir.y = -anchor.y;
        goalDir = goalDir.normalized;

        // accumulate root offset an get root position
        var rootPos = m_Root.position;

        // the maximum stride distance projected along the leg
        var goalMax = Mathf.Max(
            m_Length,
            strideLength / Mathf.Sin(Vector3.Angle(goalDir, Vector3.down))
        );

        var goalPos = rootPos + goalDir * goalMax;
        var goalRot = Quaternion.identity;

        // find foot placement
        var castSrc = rootPos;
        var castDir = goalDir;
        var castLen = goalMax;

        var didHit = Physics.Raycast(
            castSrc,
            castDir,
            out var hit,
            castLen,
            m_CastMask
        );

        if (didHit) {
            goalPos = hit.point;
            goalRot = Quaternion.LookRotation(
                Vector3.ProjectOnPlane(stride, hit.normal),
                hit.normal
            );
        }

        m_GoalPos = goalPos;
        m_GoalRot = goalRot;

        DebugDraw.PushLine(
            m_Goal.DebugName("stride-curr"),
            m_Anchor.GoalPos,
            m_GoalPos,
            new DebugDraw.Config(m_Goal.DebugColor(), tags: DebugDraw.Tag.Movement, count: 1, width: 0.03f)
        );

        // once we complete our stride, switch to hold
        if (Vector3.SqrMagnitude(stride) > strideLength * strideLength) {
            ChangeToImmediate(Holding, delta);

            DebugDraw.Push(
                m_Goal.DebugName("stride-hold"),
                m_GoalPos,
                new DebugDraw.Config(m_Goal.DebugColor(0.5f), tags: DebugDraw.Tag.Movement, width: 5f)
            );

            DebugDraw.PushLine(
                m_Goal.DebugName("stride"),
                m_Anchor.GoalPos,
                m_GoalPos,
                new DebugDraw.Config(m_Goal.DebugColor(0.5f), tags: DebugDraw.Tag.Movement, width: 0.5f)
            );
        }
    }

    // -- Holding --
    Phase Holding => new(
        name: "Holding",
        enter: Holding_Enter,
        update: Holding_Update,
        exit: Holding_Exit
    );

    void Holding_Enter() {
        m_IsHeld = true;
    }

    void Holding_Update(float delta) {
        var didHit = FindSurface(m_GoalPos - m_Offset, out var hitPoint);
        if (!didHit) {
            ChangeTo(Free);
            return;
        }

        m_GoalPos = hitPoint;
    }

    void Holding_Exit() {
        m_IsHeld = false;
    }

    // -- queries --
    /// if the limb is currently free
    public bool IsFree {
        get => m_IsFree;
    }

    /// if the limb is currently held
    public bool IsHeld {
        get => m_IsHeld;
    }

    /// the current ik position of the limb
    public Vector3 GoalPos {
        get => m_GoalPos;
    }

    /// the current ik rotation of the limb
    public Quaternion GoalRot {
        get => m_GoalRot;
    }

    /// the direction towards the surface
    Vector3 RootDir {
        get => -m_Root.up;
    }

    /// cast for a surface underneath the current pos
    bool FindSurface(Vector3 goalPos, out Vector3 pos) {
        var castDir = RootDir; // TODO: arms? maybe t.forward
        var castSrc = goalPos - castDir * m_CastOffset;
        var castLen = m_Length + m_CastOffset;

        var didHit = Physics.Raycast(
            castSrc,
            castDir,
            out var hit,
            castLen,
            m_CastMask,
            QueryTriggerInteraction.Ignore
        );

        pos = hit.point;

        return didHit;
    }
}

}