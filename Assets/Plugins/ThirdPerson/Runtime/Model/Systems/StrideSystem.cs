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

    /// if the limb is currently idle
    bool m_IsIdle;

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

    /// the current speed scale
    float m_CurrSpeedScale;

    /// the current stride length
    float m_CurrStrideLength;

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

    public override void Update(float delta) {
        var speed = c.State.Curr.SurfaceVelocity.magnitude;
        m_CurrSpeedScale = m_SpeedScale.Evaluate(speed);
        m_CurrStrideLength = m_StrideLength.Evaluate(m_CurrSpeedScale);

        base.Update(delta);
    }

    // -- commands --
    /// switch to the moving state
    public void Move(CharacterBone anchor) {
        m_Anchor = anchor;
        ChangeTo(Moving);
    }

    // -- Idle --
    Phase Idle => new(
        name: "Idle",
        enter: Idle_Enter,
        update: Idle_Update,
        exit: Idle_Exit
    );

    void Idle_Enter() {
        m_IsIdle = true;
        m_GoalPos = m_Root.position + RootDir * m_Length;
    }

    void Idle_Update(float delta) {
        var didHit = FindSurface(out var hit);
        if (didHit) {
            m_GoalPos = hit.point;
            ChangeTo(Holding);
            return;
        }

        m_GoalPos = m_Root.position + RootDir * m_Length;
    }

    void Idle_Exit() {
        m_IsIdle = false;
    }

    // -- Moving --
    Phase Moving => new(
        name: "Moving",
        update: Moving_Update
    );

    void Moving_Update(float delta) {
        // get offset to anchor and mirror it over of anchor dir
        var offset = m_Anchor.RootPos - m_Anchor.GoalPos;
        var direction = Vector3.ProjectOnPlane(offset, Vector3.up);
        var planarDirection = direction;

        direction.y = -offset.y;
        direction = direction.normalized;

        // the maximum stride distance projected along the leg
        var maxDist = Mathf.Max(
            m_Length,
            m_CurrStrideLength / Mathf.Sin(Vector3.Angle(direction, Vector3.down))
        );

        var rootPos = m_Root.position;
        var goalPos = rootPos + direction * maxDist;
        var goalRot = Quaternion.identity;

        // find foot placement
        var didHit = Physics.Raycast(
            rootPos,
            direction,
            out var hit,
            maxDist,
            m_CastMask
        );

        if (didHit) {
            goalPos = hit.point;
            goalRot = Quaternion.LookRotation(
                Vector3.ProjectOnPlane(planarDirection, hit.normal),
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
        if (Vector3.SqrMagnitude(planarDirection) > m_CurrStrideLength * m_CurrStrideLength) {
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
        var didHit = FindSurface(out var hit);
        if (!didHit) {
            ChangeTo(Idle);
            return;
        }

        m_GoalPos = hit.point;
    }

    void Holding_Exit() {
        m_IsHeld = false;
    }

    // -- queries --
    /// if the limb is currently idle
    public bool IsIdle {
        get => m_IsIdle;
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
    bool FindSurface(out RaycastHit hit) {
        var castDir = RootDir; // TODO: arms? maybe t.forward
        var castSrc = m_GoalPos - castDir * m_CastOffset;
        var castLen = m_Length + m_CastOffset;

        var didHit = Physics.Raycast(
            castSrc,
            castDir,
            out hit,
            castLen,
            m_CastMask,
            QueryTriggerInteraction.Ignore
        );

        return didHit;
    }
}

}